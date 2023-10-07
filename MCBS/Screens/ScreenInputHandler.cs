using static MCBS.Config.ConfigManager;
using CoreRCON;
using QuanLib.Minecraft.Vector;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using CoreRCON.Parsers.Standard;
using QuanLib.Minecraft.Selectors;
using QuanLib.Core;
using QuanLib.Minecraft.Command;
using QuanLib.Minecraft;
using MCBS.Events;
using MCBS.Cursor;
using QuanLib.Minecraft.Command.Senders;
using QuanLib.Minecraft.Snbt.Models;

namespace MCBS.Screens
{
    /// <summary>
    /// 屏幕输入处理
    /// </summary>
    public class ScreenInputHandler
    {
        public ScreenInputHandler(ScreenContext owner)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
            _events = new();
            IdleTime = 0;

            CursorMove += OnCursorMove;
            LeftClick += OnLeftClick;
            RightClick += OnRightClick;
            TextEditorUpdate += OnTextEditorUpdate;
            CursorSlotChanged += OnCursorSlotChanged;
            CursorItemChanged += OnCursorItemChanged;
        }

        private const string CLICK_ITEM = "minecraft:snowball";

        private const string TEXTEDITOR_ITEM = "minecraft:writable_book";

        private readonly ScreenContext _owner;

        private readonly Queue<Event> _events;

        public int IdleTime { get; private set; }

        public event EventHandler<ScreenInputHandler, CursorEventArgs> CursorMove;

        public event EventHandler<ScreenInputHandler, CursorEventArgs> LeftClick;

        public event EventHandler<ScreenInputHandler, CursorEventArgs> RightClick;

        public event EventHandler<ScreenInputHandler, CursorEventArgs> TextEditorUpdate;

        public event EventHandler<ScreenInputHandler, CursorEventArgs> CursorSlotChanged;

        public event EventHandler<ScreenInputHandler, CursorEventArgs> CursorItemChanged;

        protected virtual void OnCursorMove(ScreenInputHandler sender, CursorEventArgs e) { }

        protected virtual void OnLeftClick(ScreenInputHandler sender, CursorEventArgs e) { }

        protected virtual void OnRightClick(ScreenInputHandler sender, CursorEventArgs e) { }

        protected virtual void OnTextEditorUpdate(ScreenInputHandler sender, CursorEventArgs e) { }

        protected virtual void OnCursorSlotChanged(ScreenInputHandler sender, CursorEventArgs e) { }

        protected virtual void OnCursorItemChanged(ScreenInputHandler sender, CursorEventArgs e) { }

        public void HandleInput()
        {
            _owner.CursorManager.ResetAll();

            Screen screen = _owner.Screen;
            CommandSender sender = MCOS.Instance.MinecraftInstance.CommandSender;
            Dictionary<string, EntityPos> playerPositions = sender.GetAllPlayerPosition();
            int length = screen.Width > screen.Height ? screen.Width : screen.Height;
            BlockPos center = screen.CenterPosition;
            Vector3<double> start = new(center.X - length, center.Y - length, center.Z - length);
            Vector3<double> range = new(length * 2, length * 2, length * 2);
            Bounds bounds = new(start, range);
            Func<IVector3<double>, double> func = screen.NormalFacing switch
            {
                Facing.Xp or Facing.Xm => (positions) => Math.Abs(positions.X - screen.PlaneCoordinate),
                Facing.Yp or Facing.Ym => (positions) => Math.Abs(positions.Y - screen.PlaneCoordinate),
                Facing.Zp or Facing.Zm => (positions) => Math.Abs(positions.Z - screen.PlaneCoordinate),
                _ => throw new InvalidOperationException(),
            };

            List<(string player, double distance)> playerDistances = new();
            foreach (var playerPosition in playerPositions)
            {
                if (bounds.Contains(playerPosition.Value))
                    playerDistances.Add((playerPosition.Key, func.Invoke(playerPosition.Value)));
            }

            if (playerDistances.Count == 0)
            {
                IdleTime++;
                return;
            }

            int count = 0;
            var order = playerDistances.OrderBy(item => item.distance);
            foreach (var (player, distance) in order)
            {
                if (HandlePlayer(player))
                    count++;
            }

            if (count == 0)
            {
                IdleTime++;
                return;
            }

            while (_events.TryDequeue(out var e))
                e.Invoke();

            IdleTime = 0;
            return;
        }

        private bool HandlePlayer(string player)
        {
            Screen screen = _owner.Screen;
            CommandSender sender = MCOS.Instance.MinecraftInstance.CommandSender;
            CursorContext context = _owner.CursorManager.GetOrCreate(player);
            CursorInputData oldData = context.InputData.Clone();
            CursorMode cursorMode = oldData.CursorMode;
            Point cursorPosition = oldData.CursorPosition;
            DateTime leftClickTime = oldData.LeftClickTime;
            DateTime rightClickTime = oldData.RightClickTime;
            string textEditor = oldData.TextEditor;
            int inventorySlot = oldData.InventorySlot;
            Item? mainItem = oldData.MainItem;
            Item? deputyItem = oldData.DeputyItem;

            if (!sender.TryGetPlayerSelectedItemSlot(player, out inventorySlot))
                return false;

            sender.TryGetPlayerItem(player, inventorySlot, out mainItem);
            sender.TryGetPlayerDualWieldItem(player, out deputyItem);

            if (mainItem is not null && (mainItem.ID == CLICK_ITEM || mainItem.ID == TEXTEDITOR_ITEM))
            {

            }
            else if (deputyItem is not null && (deputyItem.ID == CLICK_ITEM || deputyItem.ID == TEXTEDITOR_ITEM))
            {
                Item? temp = mainItem;
                mainItem = deputyItem;
                deputyItem = temp;
            }
            else
            {
                return false;
            }

            if (!sender.TryGetEntityPosition(player, out var playerPosition) || !sender.TryGetEntityRotation(player, out var playerRotation))
                return false;
            if (!EntityPos.CheckPlaneReachability(playerPosition, playerRotation, screen.NormalFacing, screen.PlaneCoordinate))
                return false;

            playerPosition.Y += 1.625;
            BlockPos targetBlock = EntityPos.GetToPlaneIntersection(playerPosition, playerRotation.ToDirection(), screen.NormalFacing, screen.PlaneCoordinate).ToBlockPos();
            cursorPosition = screen.ToScreenPosition(targetBlock);

            if (!screen.IncludedOnScreen(cursorPosition))
                return false;

            if (ScreenConfig.ScreenOperatorList.Count != 0 && !ScreenConfig.ScreenOperatorList.Contains(player))
            {
                sender.ShowActionbarTitle(player, "[屏幕输入模块] 错误：你没有权限控制屏幕", TextColor.Red);
                return false;
            }

            DateTime now = DateTime.Now;
            switch (mainItem.ID)
            {
                case CLICK_ITEM:
                    cursorMode = CursorMode.Click;
                    if (sender.TryGetEntityUuid(player, out var uuid) && MCOS.Instance.InteractionManager.Items.TryGetValue(uuid, out var interaction))
                    {
                        if (interaction.IsLeftClick)
                            leftClickTime = now;
                        if (interaction.IsRightClick)
                            rightClickTime = now;
                    }
                    else
                    {
                        int score = sender.GetPlayerScoreboard(player, ScreenConfig.RightClickObjective);
                        if (score > 0)
                        {
                            rightClickTime = now;
                            sender.SetPlayerScoreboard(player, ScreenConfig.RightClickObjective, 0);
                        }
                    }
                    break;
                case TEXTEDITOR_ITEM:
                    cursorMode = CursorMode.TextEditor;
                    if (context.TextEditor.ReadText(sender, player, mainItem))
                        textEditor = context.TextEditor.CurrentText;
                    break;
                default:
                    return false;
            }

            CursorInputData newData = new(
                cursorMode,
                cursorPosition,
                leftClickTime,
                rightClickTime,
                textEditor,
                inventorySlot,
                mainItem,
                deputyItem
                );
            CursorEventArgs args = new(newData.CursorPosition, context, oldData, newData);

            int count = _events.Count;
            if (oldData.CursorPosition != newData.CursorPosition)
                _events.Enqueue(new(CursorMove, this, args));
            if (oldData.LeftClickTime != newData.LeftClickTime)
                _events.Enqueue(new(LeftClick, this, args));
            if (oldData.RightClickTime != newData.RightClickTime)
                _events.Enqueue(new(RightClick, this, args));
            if (oldData.TextEditor != newData.TextEditor)
                _events.Enqueue(new(TextEditorUpdate, this, args));
            if (oldData.InventorySlot != newData.InventorySlot)
                _events.Enqueue(new(CursorSlotChanged, this, args));
            if (!Item.EqualsID(oldData.DeputyItem, newData.DeputyItem))
                _events.Enqueue(new(CursorItemChanged, this, args));

            context.SetNewInputData(newData);
            return true;
        }

        private class Event
        {
            public Event(EventHandler<ScreenInputHandler, CursorEventArgs> handler, ScreenInputHandler sender, CursorEventArgs args)
            {
                Handler = handler ?? throw new ArgumentNullException(nameof(handler));
                Sender = sender ?? throw new ArgumentNullException(nameof(sender));
                Args = args ?? throw new ArgumentNullException(nameof(args));
            }

            public EventHandler<ScreenInputHandler, CursorEventArgs> Handler { get; }

            public ScreenInputHandler Sender { get; }

            public CursorEventArgs Args { get; }

            public void Invoke()
            {
                Handler.Invoke(Sender, Args);
            }
        }
    }
}
