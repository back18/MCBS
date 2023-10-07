using MCBS.Config;
using MCBS.Cursor;
using QuanLib.Minecraft.Command;
using QuanLib.Minecraft.Command.Senders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.RightClickObjective
{
    public class RightClickObjectiveContext
    {
        public RightClickObjectiveContext(string playerName)
        {
            if (string.IsNullOrEmpty(playerName))
                throw new ArgumentException($"“{nameof(playerName)}”不能为 null 或空。", nameof(playerName));

            PlayerName = playerName;
        }

        public string PlayerName { get; }

        public bool IsRightClick { get; private set; }

        public void Handle()
        {
            CommandSender sender = MCOS.Instance.MinecraftInstance.CommandSender;
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
