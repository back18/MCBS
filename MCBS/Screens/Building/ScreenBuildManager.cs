using static MCBS.Config.ConfigManager;
using QuanLib.Minecraft;
using QuanLib.Minecraft.Command;
using QuanLib.Minecraft.Command.Senders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuanLib.Minecraft.NBT.Models;
using QuanLib.TickLoop;
using QuanLib.Logging;
using QuanLib.Game;
using QuanLib.Core;

namespace MCBS.Screens.Building
{
    public class ScreenBuildManager : ITickUpdatable
    {
        private static readonly ILogger LOGGER = Log4NetManager.Instance.GetLogger();

        private const string AIR_BLOCK = "minecraft:air";

        public ScreenBuildManager()
        {
            _screenBuildContexts = [];

            Enable = true;
        }

        private readonly Dictionary<string, ScreenBuildContext> _screenBuildContexts;

        public int ScreenCount => MinecraftBlockScreen.Instance.ScreenManager.Collection.Count;

        public bool Enable { get; set; }

        public void OnTickUpdate(int tick)
        {
            CleanContexts();

            if (!Enable || ScreenCount >= ScreenConfig.MaxCount)
                return;

            if (tick % 20 == 0)
                SearchPlayers();

            HandleContexts(tick);
        }

        private void HandleContexts(int tick)
        {
            foreach (var context in _screenBuildContexts.Values)
            {
                context.OnTickUpdate(tick);
            }
        }

        private void CleanContexts()
        {
            CommandSender sender = MinecraftBlockScreen.Instance.MinecraftInstance.CommandSender;

            foreach (var item in _screenBuildContexts)
            {
                string playerName = item.Key;
                ScreenBuildContext screenBuildContext = item.Value;

                if (screenBuildContext.BuildState == ScreenBuildState.Canceled)
                {
                    _screenBuildContexts.Remove(playerName);
                    sender.SendChatMessage(playerName, "[屏幕构建器] 已取消", TextColor.Red);
                }
                else if (screenBuildContext.BuildState == ScreenBuildState.Completed)
                {
                    _screenBuildContexts.Remove(playerName);

                    if (!Enable)
                    {
                        sender.SendChatMessage(playerName, $"[屏幕构建器] 屏幕构建器已被禁用", TextColor.Red);
                        break;
                    }

                    if (ScreenCount >= ScreenConfig.MaxCount)
                    {
                        sender.SendChatMessage(playerName, $"[屏幕构建器] 当前世界屏幕数量已达到最大数量限制{ScreenConfig.MaxCount}个，无法继续创建屏幕", TextColor.Red);
                        break;
                    }

                    Screen screen = screenBuildContext.GetScreen();
                    CubeRange range = screen.GetRange(0, 1).Normalize();

                    if (range.StartPosition.Y < ScreenConfig.MinAltitude ||
                        range.EndPosition.Y > ScreenConfig.MaxAltitude)
                    {
                        sender.SendChatMessage(playerName, $"[屏幕构建器] 屏幕超出了世界建筑高度{ScreenConfig.MinAltitude}~{ScreenConfig.MaxAltitude}，无法创建", TextColor.Red);
                        break;
                    }

                    if (screen.Width > ScreenConfig.MaxLength ||
                        screen.Height > ScreenConfig.MaxLength)
                    {
                        sender.SendChatMessage(playerName, $"[屏幕构建器] 屏幕大于最大尺寸{ScreenConfig.MaxLength}x{ScreenConfig.MaxLength}，无法创建", TextColor.Red);
                        break;
                    }

                    if (screen.Width < ScreenConfig.MinLength ||
                        screen.Height < ScreenConfig.MinLength)
                    {
                        sender.SendChatMessage(playerName, $"[屏幕构建器] 屏幕小于最小尺寸{ScreenConfig.MinLength}x{ScreenConfig.MinLength}，无法创建", TextColor.Red);
                        break;
                    }

                    if (!sender.CheckRangeBlock(range.StartPosition, range.EndPosition, AIR_BLOCK))
                    {
                        sender.SendChatMessage(playerName, $"[屏幕构建器] 屏幕范围内包含非空气方块，无法创建", TextColor.Red);
                        break;
                    }

                    try
                    {
                        MinecraftBlockScreen.Instance.BuildScreen(screen);
                    }
                    catch (Exception ex)
                    {
                        LOGGER.Error("屏幕创建失败", ex);
                        sender.SendChatMessage(playerName, $"[屏幕构建器] 屏幕创建失败，错误信息: {ex.GetType()}: {ex.Message}", TextColor.Red);
                    }

                    sender.SendChatMessage(playerName, $"[屏幕构建器] 屏幕创建成功，位于: {screen.StartPosition}");
                }
            }
        }

        private void SearchPlayers()
        {
            CommandSender sender = MinecraftBlockScreen.Instance.MinecraftInstance.CommandSender;
            Dictionary<string, Item> PlayerSelectedItems = sender.GetAllPlayerSelectedItem();

            foreach (var PlayerSelectedItem in PlayerSelectedItems)
            {
                string playerName = PlayerSelectedItem.Key;
                Item item = PlayerSelectedItem.Value;

                if (_screenBuildContexts.ContainsKey(playerName))
                    continue;

                if (item.GetItemName() != ScreenConfig.ScreenBuilderItemName)
                    return;

                var whitelist = ScreenConfig.ScreenBuildOperatorList;
                if (whitelist.Count > 0 && !whitelist.Contains(playerName))
                {
                    sender.ShowActionbarTitle(playerName, "[屏幕构建器] 你没有权限创建屏幕", TextColor.Red);
                    continue;
                }

                _screenBuildContexts.Add(playerName, new ScreenBuildContext(playerName));
                sender.SendChatMessage(playerName, $"[屏幕构建器] 开始创建屏幕，初始尺寸{ScreenConfig.InitialWidth}x{ScreenConfig.InitialHeight}，请确定屏幕位置以及朝向，右键确定创建");
            }
        }
    }
}
