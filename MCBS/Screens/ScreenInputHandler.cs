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
using System.Diagnostics.CodeAnalysis;

namespace MCBS.Screens
{
    /// <summary>
    /// 屏幕输入处理
    /// </summary>
    public class ScreenInputHandler
    {
        public ScreenInputHandler(ScreenContext owner)
        {
            ArgumentNullException.ThrowIfNull(owner, nameof(owner));

            _owner = owner;
            IdleTime = 0;
        }

        private readonly ScreenContext _owner;

        public int IdleTime { get; private set; }

        public CursorContext[] HandleInput()
        {
            List<CursorContext> cursors = new();
            Screen screen = _owner.Screen;
            CommandSender sender = MCOS.Instance.MinecraftInstance.CommandSender;
            Dictionary<string, EntityPos> playerPositions = sender.GetAllPlayerPosition();
            int length = screen.Width > screen.Height ? screen.Width : screen.Height;
            BlockPos center = screen.CenterPosition;
            Vector3<double> start = new(center.X - length, center.Y - length, center.Z - length);
            Vector3<double> range = new(length * 2, length * 2, length * 2);
            Bounds bounds = new(start, range);
            List<(string player, double distance)> playerDistances = new();

            foreach (var item in playerPositions)
            {
                double distance = screen.GetPlaneDistance(item.Value);
                if (distance < 0)
                    continue;

                if (bounds.Contains(item.Value))
                    playerDistances.Add((item.Key, distance));
            }

            if (playerDistances.Count == 0)
            {
                IdleTime++;
                return cursors.ToArray();
            }

            var order = playerDistances.OrderBy(item => item.distance);
            foreach (var (player, distance) in order)
            {
                CursorContext cursorContext = MCOS.Instance.CursorManager.GetOrCreate(player);

                lock (cursorContext)
                {
                    if (cursorContext.Active &&
                        cursorContext.ScreenContextOf is not null &&
                        cursorContext.ScreenContextOf != _owner &&
                        cursorContext.ScreenContextOf.Screen.GetPlaneDistance(playerPositions[player]) < screen.GetPlaneDistance(playerPositions[player]))
                        continue;

                    if (HandleInput(cursorContext, out var result))
                    {
                        cursorContext.SetNewInputData(_owner, result);
                        cursors.Add(cursorContext);
                    }
                }
            }

            if (cursors.Count == 0)
            {
                IdleTime++;
                return cursors.ToArray();
            }

            IdleTime = 0;
            return cursors.ToArray();
        }

        private bool HandleInput(CursorContext cursorContext, [MaybeNullWhen(false)] out CursorInputData result)
        {
            Screen screen = _owner.Screen;
            CommandSender sender = MCOS.Instance.MinecraftInstance.CommandSender;
            CursorInputData oldData = cursorContext.NewInputData.Clone();
            CursorMode cursorMode = oldData.CursorMode;
            Point cursorPosition = oldData.CursorPosition;
            Point leftClickPosition = oldData.LeftClickPosition;
            Point rightClickPosition = oldData.RightClickPosition;
            DateTime leftClickTime = oldData.LeftClickTime;
            DateTime rightClickTime = oldData.RightClickTime;
            string textEditor = oldData.TextEditor;
            int inventorySlot = oldData.InventorySlot;
            Item? mainItem = oldData.MainItem;
            Item? deputyItem = oldData.DeputyItem;
            Item? toolsItem = null;

            if (!sender.TryGetPlayerSelectedItemSlot(cursorContext.PlayerName, out inventorySlot))
                goto fail;

            sender.TryGetPlayerItem(cursorContext.PlayerName, inventorySlot, out mainItem);
            sender.TryGetPlayerDualWieldItem(cursorContext.PlayerName, out deputyItem);

            if (mainItem is not null && (mainItem.ID == ScreenConfig.RightClickItemID || mainItem.ID == ScreenConfig.TextEditorItemID))
                toolsItem = mainItem;
            else if (deputyItem is not null && (deputyItem.ID == ScreenConfig.RightClickItemID || deputyItem.ID == ScreenConfig.TextEditorItemID))
                toolsItem = deputyItem;

            if (toolsItem is null)
                goto fail;

            if (!sender.TryGetEntityPosition(cursorContext.PlayerName, out var playerPosition) || !sender.TryGetEntityRotation(cursorContext.PlayerName, out var playerRotation))
                goto fail;
            if (!EntityPos.CheckPlaneReachability(playerPosition, playerRotation, screen.NormalFacing, screen.PlaneCoordinate))
                goto fail;

            playerPosition.Y += 1.625;
            BlockPos targetBlock = EntityPos.GetToPlaneIntersection(playerPosition, playerRotation.ToDirection(), screen.NormalFacing, screen.PlaneCoordinate).ToBlockPos();
            cursorPosition = screen.WorldPos2ScreenPos(targetBlock);

            if (!screen.IncludedOnScreen(cursorPosition))
                goto fail;

            if (ScreenConfig.ScreenOperatorList.Count != 0 && !ScreenConfig.ScreenOperatorList.Contains(cursorContext.PlayerName))
            {
                sender.ShowActionbarTitle(cursorContext.PlayerName, "[屏幕输入模块] 错误：你没有权限控制屏幕", TextColor.Red);
                goto fail;
            }

            if (toolsItem.ID == ScreenConfig.RightClickItemID)
            {
                cursorMode = CursorMode.Click;
                ClickResult clickResult = cursorContext.ClickReader.ReadClick();
                if (clickResult.IsLeftClick)
                {
                    leftClickPosition = cursorPosition;
                    leftClickTime = cursorContext.ClickReader.LeftClickTime;
                }
                if (clickResult.IsRightClick)
                {
                    rightClickPosition = cursorPosition;
                    rightClickTime = cursorContext.ClickReader.RightClickTime;
                }
            }
            else if (toolsItem.ID == ScreenConfig.TextEditorItemID)
            {
                cursorMode = CursorMode.TextEditor;
                cursorContext.TextEditor.ReadText(toolsItem);
                textEditor = cursorContext.TextEditor.CurrentText;
            }
            else
            {
                goto fail;
            }

            result = new(
                cursorMode,
                cursorPosition,
                leftClickPosition,
                rightClickPosition,
                leftClickTime,
                rightClickTime,
                textEditor,
                inventorySlot,
                mainItem,
                deputyItem
                );
            return true;

            fail:
            result = null;
            return false;
        }
    }
}
