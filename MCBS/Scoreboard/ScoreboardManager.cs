using MCBS.Config;
using QuanLib.Minecraft;
using QuanLib.Minecraft.Command;
using QuanLib.Minecraft.Command.Senders;
using QuanLib.Minecraft.Instance;
using QuanLib.TickLoop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Objective
{
    public class ScoreboardManager : ITickUpdatable
    {
        public ScoreboardManager()
        {
            _items = new();
        }

        private readonly Dictionary<string, ScoreboardContext> _items;


        public void Initialize()
        {
            CommandSender sender = MinecraftBlockScreen.Instance.MinecraftInstance.CommandSender;
            sender.SendCommand($"scoreboard objectives remove {ConfigManager.ScreenConfig.RightClickObjective}");
            sender.SendCommand($"scoreboard objectives add {ConfigManager.ScreenConfig.RightClickObjective} {ConfigManager.ScreenConfig.RightClickCriterion}");
        }

        public void OnTickUpdate(int tick)
        {
            PlayerList playerList = MinecraftBlockScreen.Instance.MinecraftInstance.CommandSender.GetPlayerList();
            _items.Clear();
            foreach (var player in playerList.List)
            {
                ScoreboardContext context = new(player);
                context.OnTickUpdate(tick);
                _items.Add(player, context);
            }
        }

        public bool IsRightClick(string player)
        {
            ArgumentException.ThrowIfNullOrEmpty(player, nameof(player));

            if (_items.TryGetValue(player, out var context))
                return context.IsRightClick;
            else
                return false;
        }
    }
}
