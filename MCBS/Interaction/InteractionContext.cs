using FFmpeg.AutoGen;
using log4net.Core;
using MCBS.Directorys;
using Newtonsoft.Json;
using QuanLib.Core;
using QuanLib.IO;
using QuanLib.Logging;
using QuanLib.Minecraft;
using QuanLib.Minecraft.Command;
using QuanLib.Minecraft.Command.Senders;
using QuanLib.Minecraft.Vector;
using QuanLib.TickLoop;
using QuanLib.TickLoop.StateMachine;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Interaction
{
    public class InteractionContext : UnmanagedBase, ITickUpdatable
    {
        private static readonly LogImpl LOGGER = LogManager.Instance.GetLogger();
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

            StateMachine = new(InteractionState.NotCreated, new StateContext<InteractionState>[]
            {
                new(InteractionState.NotCreated, Array.Empty<InteractionState>(), GotoNotCreatedState),
                new(InteractionState.Active,  new InteractionState[] { InteractionState.NotCreated }, GotoActiveState, ActiveStateUpdate),
                new(InteractionState.Offline, new InteractionState[] { InteractionState.Active }, GotoOfflineState),
                new(InteractionState.Closed, new InteractionState[] { InteractionState.Active, InteractionState.Offline }, GotoClosedState)
            });

            SaveJson();
        }

        public TickStateMachine<InteractionState> StateMachine { get; }

        public InteractionState InteractionState => StateMachine.CurrentState;

        public string PlayerName { get; }

        public Guid PlayerUUID { get; private set; }

        public Guid EntityUUID { get; private set; }

        public EntityPos Position { get; private set; }

        public long LeftClickTimestamp { get; private set; }

        public long RightClickTimestamp { get; private set; }

        public bool IsLeftClick { get; private set; }

        public bool IsRightClick { get; private set; }

        public void OnTickUpdate(int tick)
        {
            StateMachine.OnTickUpdate(tick);
        }

        protected virtual bool GotoNotCreatedState(InteractionState sourceState, InteractionState targetState)
        {
            return false;
        }

        protected virtual bool GotoActiveState(InteractionState sourceState, InteractionState targetState)
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

        protected virtual bool GotoOfflineState(InteractionState sourceState, InteractionState targetState)
        {
            return !ConditionalEntity();
        }

        protected virtual bool GotoClosedState(InteractionState sourceState, InteractionState targetState)
        {
            Dispose();
            return true;
        }

        protected virtual void ActiveStateUpdate(int tick)
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
            StateMachine.Submit(InteractionState.Active);
        }

        public void CloseInteraction()
        {
            StateMachine.Submit(InteractionState.Closed);
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
