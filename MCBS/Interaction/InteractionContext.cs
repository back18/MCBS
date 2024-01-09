using FFmpeg.AutoGen;
using log4net.Core;
using MCBS.Directorys;
using MCBS.Logging;
using MCBS.State;
using Newtonsoft.Json;
using QuanLib.Core;
using QuanLib.IO;
using QuanLib.Minecraft;
using QuanLib.Minecraft.Command;
using QuanLib.Minecraft.Command.Senders;
using QuanLib.Minecraft.Selectors;
using QuanLib.Minecraft.Vector;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Interaction
{
    public class InteractionContext : UnmanagedBase, ITickable
    {
        private static readonly LogImpl LOGGER = LogUtil.GetLogger();
        private const string INTERACTION_ID = "minecraft:interaction";
        private const string INTERACTION_NBT = "{width:3,height:3,response:true}";

        public InteractionContext(string playerName)
        {
            ArgumentException.ThrowIfNullOrEmpty(playerName, nameof(playerName));

            PlayerName = playerName;
            PlayerUUID = Guid.Empty;
            EntityUUID = Guid.Empty;
            Position = new(0, 0, 0);
            LeftClickTimestamp = 0;
            RightClickTimestamp = 0;
            IsLeftClick = false;
            IsRightClick = false;

            StateManager = new(InteractionState.NotCreated, new StateContext<InteractionState>[]
            {
                new(InteractionState.NotCreated, Array.Empty<InteractionState>(), HandleNotCreatedState),
                new(InteractionState.Active,  new InteractionState[] { InteractionState.NotCreated }, HandleActiveState, OnActiveState),
                new(InteractionState.Offline, new InteractionState[] { InteractionState.Active }, HandleOfflineState),
                new(InteractionState.Closed, new InteractionState[] { InteractionState.Active, InteractionState.Offline }, HandleClosedState)
            });

            SaveJson();
        }

        public StateManager<InteractionState> StateManager { get; }

        public InteractionState InteractionState => StateManager.CurrentState;

        public string PlayerName { get; }

        public Guid PlayerUUID { get; private set; }

        public Guid EntityUUID { get; private set; }

        public EntityPos Position { get; private set; }

        public long LeftClickTimestamp { get; private set; }

        public long RightClickTimestamp { get; private set; }

        public bool IsLeftClick { get; private set; }

        public bool IsRightClick { get; private set; }

        public void OnTick()
        {
            StateManager.HandleAllState();
        }

        protected virtual bool HandleNotCreatedState(InteractionState current, InteractionState next)
        {
            return false;
        }

        protected virtual bool HandleActiveState(InteractionState current, InteractionState next)
        {
            CommandSender sender = MinecraftBlockScreen.Instance.MinecraftInstance.CommandSender;

            if (!sender.TryGetEntityUuid(PlayerName, out var playerUUID))
                return false;

            if (!sender.TryGetEntityPosition(playerUUID.ToString(), out var position))
                return false;

            if (sender.ConditionalEntity($"@e[limit=1,type=minecraft:interaction,x={position.X},y={position.Y},z={position.Z},distance=..1,sort=nearest]"))
                return false;

            if (!sender.SummonEntity(position, INTERACTION_ID, INTERACTION_NBT))
                return false;

            if (!sender.TryGetEntityUuid($"@e[limit=1,type=minecraft:interaction,x={position.X},y={position.Y},z={position.Z},distance=..1,sort=nearest]", out var entityUUID))
                return false;

            PlayerUUID = playerUUID;
            EntityUUID = entityUUID;
            Position = position;
            LOGGER.Info($"交互实体({EntityUUID})已和玩家({PlayerUUID})绑定");
            return true;
        }

        protected virtual bool HandleOfflineState(InteractionState current, InteractionState next)
        {
            return !ConditionalEntity();
        }

        protected virtual bool HandleClosedState(InteractionState current, InteractionState next)
        {
            Dispose();
            return true;
        }

        protected virtual void OnActiveState()
        {
            if (!ConditionalEntity())
            {
                LOGGER.Info($"交互实体({EntityUUID})或玩家({PlayerUUID})已离线，即将解绑");
                CloseInteraction();
                return;
            }

            ReadLeftRightKeys();
            SyncPosition();
            SaveJson();
        }

        public void CreateInteraction()
        {
            StateManager.AddNextState(InteractionState.Active);
        }

        public void CloseInteraction()
        {
            StateManager.AddNextState(InteractionState.Closed);
        }

        public bool ConditionalEntity()
        {
            CommandSender sender = MinecraftBlockScreen.Instance.MinecraftInstance.CommandSender;
            return sender.ConditionalEntity(PlayerUUID.ToString()) && sender.ConditionalEntity(EntityUUID.ToString());
        }

        public bool SyncPosition()
        {
            CommandSender sender = MinecraftBlockScreen.Instance.MinecraftInstance.CommandSender;
            if (!sender.TryGetEntityPosition(PlayerUUID.ToString(), out var position))
                return false;

            Position = position;
            return sender.TelePort(EntityUUID.ToString(), Position) > 0;
        }

        public void ReadLeftRightKeys()
        {
            IsLeftClick = false;
            IsRightClick = false;
            CommandSender sender = MinecraftBlockScreen.Instance.MinecraftInstance.CommandSender;
            LeftRightKeys keys = sender.GetInteractionData(EntityUUID.ToString());

            if (keys.LeftClick.Timestamp > LeftClickTimestamp)
            {
                LeftClickTimestamp = keys.LeftClick.Timestamp;
                if (keys.LeftClick.Player == PlayerUUID)
                    IsLeftClick = true;
            }
            if (keys.RightClick.Timestamp > RightClickTimestamp)
            {
                RightClickTimestamp = keys.RightClick.Timestamp;
                if (keys?.RightClick.Player == PlayerUUID)
                    IsRightClick = true;
            }
        }

        private void SaveJson()
        {
            MinecraftBlockScreen.Instance.FileWriteQueue.Submit(new TextWriteTask(GetSavePath(), ToJson()));
        }

        private void DeleteJson()
        {
            string savePath = GetSavePath();
            if (File.Exists(savePath))
                File.Delete(savePath);
        }

        private string ToJson()
        {
            return JsonConvert.SerializeObject(ToModel());
        }

        private Model ToModel()
        {
            return new(PlayerUUID.ToString(), EntityUUID.ToString(), [Position.X, Position.Y, Position.Z]);
        }

        private string GetSavePath()
        {
            return MinecraftBlockScreen.Instance.WorldDirectory.GetMcbsDataDirectory().InteractionsDir.Combine(PlayerName + ".json");
        }

        protected override void DisposeUnmanaged()
        {
            CommandSender sender = MinecraftBlockScreen.Instance.MinecraftInstance.CommandSender;
            BlockPos blockPos = Position.ToBlockPos();
            sender.AddForceloadChunk(blockPos);
            sender.KillEntity(EntityUUID.ToString());
            sender.RemoveForceloadChunk(blockPos);
            DeleteJson();
            LOGGER.Info($"交互实体({EntityUUID})已和玩家({PlayerUUID})解绑");
        }

        public class Model
        {
            public Model(string playerUUID, string entityUUID, double[] position)
            {
                ArgumentException.ThrowIfNullOrEmpty(playerUUID, nameof(playerUUID));
                ArgumentException.ThrowIfNullOrEmpty(entityUUID, nameof(entityUUID));
                ArgumentNullException.ThrowIfNull(position, nameof(position));

                PlayerUUID = playerUUID;
                EntityUUID = entityUUID;
                Position = position;
            }

            public string PlayerUUID { get; set; }

            public string EntityUUID { get; set; }

            public double[] Position { get; set; }
        }
    }
}
