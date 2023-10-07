using static MCBS.Config.ConfigManager;
using QuanLib.Minecraft.Vector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoreRCON.Parsers.Standard;
using Newtonsoft.Json.Linq;
using QuanLib.Minecraft.Selectors;
using System.Numerics;
using QuanLib.Minecraft.Command;
using QuanLib.Minecraft;
using QuanLib.Minecraft.Command.Senders;

namespace MCBS.Screens
{
    public class ScreenBuildContext
    {
        public ScreenBuildContext(string playerName)
        {
            if (string.IsNullOrEmpty(playerName))
                throw new ArgumentException($"“{nameof(playerName)}”不能为 null 或空。", nameof(playerName));

            PlayerName = playerName;
            BuildState = ScreenBuildState.ReadStartPosition;
            Timeout = ScreenConfig.ScreenBuildTimeout;
            Error = false;
        }

        public string PlayerName { get; }

        public Screen? Screen { get; private set; }

        public ScreenBuildState BuildState { get; private set; }

        public Facing NormalFacing { get; private set; }

        public int PlaneCoordinate { get; private set; }

        public BlockPos StartPosition { get; private set; }

        public BlockPos EndPosition { get; private set; }

        public int Timeout { get; private set; }

        public bool Error { get; private set; }

        public void Handle()
        {
            CommandSender sender = MCOS.Instance.MinecraftInstance.CommandSender;

            if (BuildState == ScreenBuildState.Timedout || BuildState == ScreenBuildState.Canceled || BuildState == ScreenBuildState.Completed)
            {
                return;
            }

            if (ScreenConfig.ScreenBuildTimeout != -1 && Timeout <= 0)
            {
                BuildState = ScreenBuildState.Timedout;
                return;
            }

            if (!sender.TryGetEntityPosition(PlayerName, out var position) ||
                !sender.TryGetEntityRotation(PlayerName, out var rotation))
            {
                BuildState = ScreenBuildState.Canceled;
                return;
            }

            if (sender.TryGetPlayerSelectedItem(PlayerName, out var item) &&
                item.ID == ScreenConfig.RightClickItemID &&
                item.Tag is not null &&
                item.Tag.TryGetValue("display", out var display) &&
                display is Dictionary<string, object> displayTag &&
                displayTag.TryGetValue("Name", out var name) &&
                name is string nameString)
            {
                JObject nameJson;
                try
                {
                    nameJson = JObject.Parse(nameString);
                }
                catch
                {
                    TryCancel();
                    return;
                }

                string? text = nameJson["text"]?.Value<string>();
                if (text is null || text != ScreenConfig.ScreenBuilderItemName)
                {
                    TryCancel();
                    return;
                }

                position.Y += 1.625;
                BlockPos targetPosition;
                if (BuildState == ScreenBuildState.ReadStartPosition)
                {
                    int distance = 1;
                    if (sender.TryGetPlayerDualWieldItem(PlayerName, out var dualWieldItem))
                        distance += dualWieldItem.Count * 4;

                    Facing playerFacing;
                    if (rotation.Pitch <= -60 || rotation.Pitch >= 60)
                        playerFacing = rotation.PitchFacing;
                    else
                        playerFacing = rotation.YawFacing;

                    var target = playerFacing switch
                    {
                        Facing.Yp => position.Y + distance,
                        Facing.Ym => position.Y - distance,
                        Facing.Xp => position.X + distance,
                        Facing.Xm => position.X - distance,
                        Facing.Zp => position.Z + distance,
                        Facing.Zm => position.Z - distance,
                        _ => throw new InvalidOperationException(),
                    };

                    NormalFacing = playerFacing.ToReverse();
                    PlaneCoordinate = (int)Math.Round(target, MidpointRounding.ToNegativeInfinity);
                    targetPosition = EntityPos.GetToPlaneIntersection(position, rotation.ToDirection(), NormalFacing, PlaneCoordinate).ToBlockPos();

                    if (targetPosition.Y < ScreenConfig.MinY || targetPosition.Y > ScreenConfig.MaxY)
                    {
                        sender.ShowActionbarTitle(PlayerName, $"[屏幕构建器] 错误：目标位置的Y轴需要在{ScreenConfig.MinY}至{ScreenConfig.MaxY}之间", TextColor.Red);
                        Error = true;
                        goto click;
                    }

                    sender.ShowTitle(PlayerName, 0, 10, 10, "正在创建屏幕");
                    sender.ShowSubTitle(PlayerName, 0, 10, 10, $"方向:{playerFacing.ToReverse()} 距离:{distance} 目标位置:{targetPosition}");
                }
                else if (BuildState == ScreenBuildState.ReadEndPosition)
                {
                    targetPosition = EntityPos.GetToPlaneIntersection(position, rotation.ToDirection(), NormalFacing, PlaneCoordinate).ToBlockPos();

                    if (targetPosition.Y < ScreenConfig.MinY || targetPosition.Y > ScreenConfig.MaxY)
                    {
                        sender.ShowActionbarTitle(PlayerName, $"[屏幕构建器] 错误：目标位置的Y轴需要在{ScreenConfig.MinY}至{ScreenConfig.MaxY}之间", TextColor.Red);
                        Error = true;
                        goto click;
                    }

                    Screen? newScreen = Screen;
                    if (EndPosition != targetPosition)
                    {
                        newScreen = Screen.CreateScreen(StartPosition, targetPosition, NormalFacing);
                    }

                    if (newScreen is null)
                        goto click;

                    if (Screen.Replace(Screen, newScreen, true))
                    {
                        if (EndPosition != targetPosition)
                        {
                            Screen = newScreen;
                            EndPosition = targetPosition;
                        }
                    }
                    else
                    {
                        sender.ShowActionbarTitle(PlayerName, $"[屏幕构建器] 错误：所选范围内含有非空气方块", TextColor.Red);
                        Error = true;
                        goto click;
                    }

                    if (newScreen.Width > ScreenConfig.MaxLength || newScreen.Height > ScreenConfig.MaxLength)
                    {
                        sender.ShowActionbarTitle(PlayerName, $"[屏幕构建器] 错误：屏幕最大长度为{ScreenConfig.MaxLength}", TextColor.Red);
                        Error = true;
                        goto click;
                    }
                    else if (newScreen.TotalPixels > ScreenConfig.MaxPixels)
                    {
                        sender.ShowActionbarTitle(PlayerName, $"[屏幕构建器] 错误：屏幕最大像素数量为{ScreenConfig.MaxLength}", TextColor.Red);
                        Error = true;
                        goto click;
                    }
                    else if (newScreen.Width < ScreenConfig.MinLength || newScreen.Height < ScreenConfig.MinLength)
                    {
                        sender.ShowActionbarTitle(PlayerName, $"[屏幕构建器] 错误：屏幕最小长度为{ScreenConfig.MinLength}", TextColor.Red);
                        Error = true;
                    }
                    else if (newScreen.TotalPixels < ScreenConfig.MinPixels)
                    {
                        sender.ShowActionbarTitle(PlayerName, $"[屏幕构建器] 错误：屏幕最小像素数量为{ScreenConfig.MinPixels}", TextColor.Red);
                        Error = true;
                    }

                    sender.ShowTitle(PlayerName, 0, 10, 10, "正在确定屏幕尺寸");
                    sender.ShowSubTitle(PlayerName, 0, 10, 10, $"宽度:{Screen?.Width ?? 0} 高度:{Screen?.Height ?? 0} 像素数量: {Screen?.TotalPixels ?? 0}");
                }
                else
                {
                    throw new InvalidOperationException();
                }

                click:
                if (MCOS.Instance.CursorManager.GetOrCreate(PlayerName).ClickReader.ReadClick().IsRightClick)
                {
                    if (Error)
                    {
                        sender.SendChatMessage(PlayerName, "[屏幕构建器] 出现一个或多个错误，无法创建屏幕", TextColor.Red);
                        return;
                    }

                    if (BuildState == ScreenBuildState.ReadStartPosition)
                    {
                        StartPosition = targetPosition;
                        BuildState = ScreenBuildState.ReadEndPosition;
                        sender.SendChatMessage(PlayerName, $"[屏幕构建器] 屏幕左上角已确定，位于{StartPosition}");
                    }
                    else if (BuildState == ScreenBuildState.ReadEndPosition && Screen is not null)
                    {
                        sender.SendChatMessage(PlayerName, $"[屏幕构建器] 屏幕右下角已确定，位于{EndPosition}");
                        BuildState = ScreenBuildState.Completed;
                    }
                    else
                    {
                        throw new InvalidOperationException();
                    }
                }
            }
            else
            {
                TryCancel();
                return;
            }

            Timeout = ScreenConfig.ScreenBuildTimeout;
            Error = false;

            void TryCancel()
            {
                if (BuildState == ScreenBuildState.ReadEndPosition)
                {
                    if (ScreenConfig.ScreenBuildTimeout == -1)
                    {
                        sender.ShowActionbarTitle(PlayerName, $"[屏幕构建器] 你有一个屏幕未完成创建， 位于{StartPosition}", TextColor.Red);
                    }
                    else
                    {
                        Timeout--;
                        sender.ShowActionbarTitle(PlayerName, $"[屏幕构建器] {Timeout / 20}秒后无操作将取消本次屏幕创建", TextColor.Red);
                    }
                }
                else
                {
                    BuildState = ScreenBuildState.Canceled;
                }
            }
        }
    }
}
