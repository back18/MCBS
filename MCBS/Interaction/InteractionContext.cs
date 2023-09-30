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
    public class InteractionContext : IDisposable
    {
        private static readonly LogImpl LOGGER = LogUtil.GetLogger();
        private const string INTERACTION_ID = "minecraft:interaction";
        private const string INTERACTION_NBT = "{width:3,height:3,response:true}";

        private InteractionContext(Guid playerUUID, Guid entityUUID, EntityPos position)
        {
            PlayerUUID = playerUUID;
            EntityUUID = entityUUID;
            Position = position;

            StateManager = new(InteractionState.Active, new StateContext<InteractionState>[]
            {
                new(InteractionState.Active, Array.Empty<InteractionState>(), HandleActiveState),
                new(InteractionState.Offline, new InteractionState[] { InteractionState.Active }, HandleOfflineState),
                new(InteractionState.Closed, new InteractionState[] { InteractionState.Active, InteractionState.Offline }, HandleClosedState)
            });

            _lock = new();
            isDisposed = false;
            _player = PlayerUUID.ToString();
            _entity = EntityUUID.ToString();
            _directory = MCOS.Instance.MinecraftInstance.MinecraftDirectory.GetActiveWorldDirectory()?.GetMcbsSavesDirectory()?.InteractionsDir ?? throw new InvalidOperationException("找不到交互实体数据文件夹");
            _file = _directory.Combine(_player + ".json");
            _task = SaveJsonAsync();

            LOGGER.Info($"交互实体({_entity})已和玩家({_player})绑定");
        }

        private readonly object _lock;

        private bool isDisposed;

        private readonly string _player;

        private readonly string _entity;

        private readonly InteractionsDirectory _directory;

        private readonly string _file;

        private Task _task;

        public StateManager<InteractionState> StateManager { get; }

        public InteractionState InteractionState => StateManager.CurrentState;

        public Guid PlayerUUID { get; }

        public Guid EntityUUID { get; }

        public EntityPos Position { get; private set; }

        public long LeftClickTimestamp { get; private set; }

        public long RightClickTimestamp { get; private set; }

        public bool IsLeftClick { get; private set; }

        public bool IsRightClick { get; private set; }

        public void Handle()
        {
            if (!ConditionalEntity())
            {
                CloseInteraction();
                LOGGER.Info($"交互实体({_entity})或玩家({_player})已离线");
            }
            StateManager.HandleAllState();
            if (InteractionState == InteractionState.Active)
            {
                ReadLeftRightKeys();
                SyncPosition();
                _task = SaveJsonAsync();
            }
        }

        protected virtual bool HandleActiveState(InteractionState current, InteractionState next)
        {
            return false;
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

        public void CloseInteraction()
        {
            StateManager.AddNextState(InteractionState.Closed);
        }

        public bool ConditionalEntity()
        {
            CommandSender sender = MCOS.Instance.MinecraftInstance.CommandSender;
            return sender.ConditionalEntity(_player) && sender.ConditionalEntity(_entity);
        }

        public bool SyncPosition()
        {
            CommandSender sender = MCOS.Instance.MinecraftInstance.CommandSender;
            if (!sender.TryGetEntityPosition(_player, out var position))
                return false;

            Position = position;
            return sender.TelePort(_entity, Position) > 0;
        }

        public void ReadLeftRightKeys()
        {
            IsLeftClick = false;
            IsRightClick = false;
            CommandSender sender = MCOS.Instance.MinecraftInstance.CommandSender;
            LeftRightKeys keys = sender.GetInteractionData(_entity);

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
            return new(_player, _entity, new double[] { Position.X, Position.Y, Position.Z });
        }

        public static bool TryCreate(Guid playerUUID, [MaybeNullWhen(false)] out InteractionContext result)
        {
            CommandSender sender = MCOS.Instance.MinecraftInstance.CommandSender;

            if (!sender.TryGetEntityPosition(playerUUID.ToString(), out var position))
                goto fail;

            if (sender.ConditionalEntity($"@e[limit=1,type=minecraft:interaction,x={position.X},y={position.Y},z={position.Z},distance=..1,sort=nearest]"))
                goto fail;

            if (!sender.SummonEntity(position, INTERACTION_ID, INTERACTION_NBT))
                goto fail;

            if (!sender.TryGetEntityUuid($"@e[limit=1,type=minecraft:interaction,x={position.X},y={position.Y},z={position.Z},distance=..1,sort=nearest]", out var entityUUID))
                goto fail;

            result = new(playerUUID, entityUUID, position);
            return true;

            fail:
            result = null;
            return false;
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
                sender.KillEntity(_entity);
                sender.RemoveForceloadChunk(blockPos);
                DaleteJson();
                isDisposed = true;
                LOGGER.Info($"交互实体({_entity})已和玩家({_player})解绑");
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
                if (string.IsNullOrEmpty(playerUUID))
                    throw new ArgumentException($"“{nameof(playerUUID)}”不能为 null 或空。", nameof(playerUUID));
                if (string.IsNullOrEmpty(entityUUID))
                    throw new ArgumentException($"“{nameof(entityUUID)}”不能为 null 或空。", nameof(entityUUID));
                if (position is null)
                    throw new ArgumentNullException(nameof(position));

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
