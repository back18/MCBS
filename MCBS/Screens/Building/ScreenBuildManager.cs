using static MCBS.Config.ConfigManager;
using log4net.Core;
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

namespace MCBS.Screens.Building
{
    public class ScreenBuildManager : ITickUpdatable
    {
        private static readonly LogImpl LOGGER = LogManager.Instance.GetLogger();

        public ScreenBuildManager()
        {
            _contexts = [];

            Enable = true;
        }

        private readonly Dictionary<string, ScreenBuildContext> _contexts;

        public bool Enable { get; set; }

        public void OnTickUpdate(int tick)
        {
            CommandSender sender = MinecraftBlockScreen.Instance.MinecraftInstance.CommandSender;
            foreach (var context in _contexts.ToArray())
            {
                switch (context.Value.BuildState)
                {
                    case ScreenBuildState.Canceled:
                        _contexts.Remove(context.Key);
                        sender.SendChatMessage(context.Key, "[屏幕构建器] 已取消", TextColor.Red);
                        break;
                    case ScreenBuildState.Completed:
                        _contexts.Remove(context.Key);
                        Screen screen = context.Value.Screen;
                        if (MinecraftBlockScreen.Instance.ScreenManager.Items.Count >= ScreenConfig.MaxCount)
                        {
                            sender.SendChatMessage(context.Key, $"[屏幕构建器] 当前屏幕数量达到最大数量限制{ScreenConfig.MaxCount}个，无法继续创建屏幕", TextColor.Red);
                            break;
                        }
                        try
                        {
                            MinecraftBlockScreen.Instance.BuildScreen(screen);
                            sender.SendChatMessage(context.Key, $"[屏幕构建器] 屏幕构建成功，位于: {screen.StartPosition}");
                        }
                        catch (Exception ex)
                        {
                            LOGGER.Error("屏幕构建失败", ex);
                            sender.SendChatMessage(context.Key, $"[屏幕构建器] 屏幕构建失败，错误信息: {ex.GetType()}: {ex.Message}", TextColor.Red);
                        }
                        break;
                }
            }

            if (!Enable || MinecraftBlockScreen.Instance.ScreenManager.Items.Count >= ScreenConfig.MaxCount)
                return;

            Dictionary<string, Item> items = sender.GetAllPlayerSelectedItem();
            foreach (var item in items)
            {
                if (_contexts.ContainsKey(item.Key))
                    continue;

                if (item.Value.GetItemName() != ScreenConfig.ScreenBuilderItemName)
                    return;

                if (ScreenConfig.ScreenBuildOperatorList.Count != 0 && !ScreenConfig.ScreenBuildOperatorList.Contains(item.Key))
                {
                    sender.ShowActionbarTitle(item.Key, "[屏幕构建器] 你没有权限创建屏幕", TextColor.Red);
                    continue;
                }

                _contexts.Add(item.Key, new(item.Key));
                sender.SendChatMessage(item.Key, "[屏幕构建器] 开始创建屏幕，请确定屏幕位置以及朝向，右键确定创建");
            }

            foreach (var context in _contexts.Values)
            {
                context.OnTickUpdate(tick);
            }
        }
    }
}
