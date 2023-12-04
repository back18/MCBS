using log4net.Core;
using MCBS.Directorys;
using MCBS.Logging;
using MCBS.State;
using Newtonsoft.Json;
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
    public class InteractionContext : ITickable, IDisposable
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

            _lock = new();
            isDisposed = false;
            _directory = MCOS.Instance.MinecraftInstance.MinecraftDirectory.GetActiveWorldDirectory()?.GetMcbsDataDirectory()?.InteractionsDir ?? throw new InvalidOperationException("找不到交互实体数据文件夹");
            _file = _directory.Combine(PlayerName + ".json");
            _task = SaveJsonAsync();
        }

        private readonly object _lock;

        private bool isDisposed;

        private readonly InteractionsDirectory _directory;

        private readonly string _file;

        private Task _task;

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
            CommandSender sender = MCOS.Instance.MinecraftInstance.CommandSender;

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
            _task = SaveJsonAsync();
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
            CommandSender sender = MCOS.Instance.MinecraftInstance.CommandSender;
            return sender.ConditionalEntity(PlayerUUID.ToString()) && sender.ConditionalEntity(EntityUUID.ToString());
        }

        public bool SyncPosition()
        {
            CommandSender sender = MCOS.Instance.MinecraftInstance.CommandSender;
            if (!sender.TryGetEntityPosition(PlayerUUID.ToString(), out var position))
                return false;

            Position = position;
            return sender.TelePort(EntityUUID.ToString(), Position) > 0;
        }

        public void ReadLeftRightKeys()
        {
            IsLeftClick = false;
            IsRightClick = false;
            CommandSender sender = MCOS.Instance.MinecraftInstance.CommandSender;
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

        private async Task SaveJsonAsync()
        {
            _task?.Wait();
            _directory.CreateIfNotExists();
            await File.WriteAllTextAsync(_file, JsonConvert.SerializeObject(ToJson()));
        }

        private void DaleteJson()
        {
            if (File.Exists(_file))
                File.Delete(_file);
        }

        private Json ToJson()
        {
            return new(PlayerUUID.ToString(), EntityUUID.ToString(), new double[] { Position.X, Position.Y, Position.Z });
        }

        protected void Dispose(bool disposing)
        {
            lock (_lock)
            {
                if (isDisposed || !disposing)
                    return;

                CommandSender sender = MCOS.Instance.MinecraftInstance.CommandSender;
                BlockPos blockPos = Position.ToBlockPos();
                sender.AddForceloadChunk(blockPos);
                sender.KillEntity(EntityUUID.ToString());
                sender.RemoveForceloadChunk(blockPos);
                DaleteJson();
                isDisposed = true;
                LOGGER.Info($"交互实体({EntityUUID})已和玩家({PlayerUUID})解绑");
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        ~InteractionContext()
        {
            Dispose(disposing: false);
        }

        public class Json
        {
            public Json(string playerUUID, string entityUUID, double[] position)
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
