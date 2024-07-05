using MCBS.Config;
using MCBS.Cursor;
using QuanLib.Minecraft.Command;
using QuanLib.Minecraft.Command.Senders;
using QuanLib.TickLoop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Scoreboard
{
    public class ScoreboardContext : ITickUpdatable
    {
        public ScoreboardContext(string playerName)
        {
            ArgumentException.ThrowIfNullOrEmpty(playerName, nameof(playerName));

            PlayerName = playerName;
        }

        public string PlayerName { get; }

        public bool IsRightClick { get; private set; }

        public void OnTickUpdate(int tick)
        {
            CommandSender sender = MinecraftBlockScreen.Instance.MinecraftInstance.CommandSender;
            int score = sender.GetPlayerScoreboard(PlayerName, ConfigManager.ScreenConfig.RightClickObjective);
            if (score > 0)
            {
                sender.SetPlayerScoreboard(PlayerName, ConfigManager.ScreenConfig.RightClickObjective, 0);
                IsRightClick = true;
            }
            else
            {
                IsRightClick = false;
            }
        }
    }
}
