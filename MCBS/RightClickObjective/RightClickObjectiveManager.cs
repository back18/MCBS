using MCBS.Config;
using QuanLib.Minecraft;
using QuanLib.Minecraft.Command;
using QuanLib.Minecraft.Command.Senders;
using QuanLib.Minecraft.Instance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.RightClickObjective
{
    public class RightClickObjectiveManager : ITickable
    {
        public RightClickObjectiveManager()
        {
            _items = new();
        }

        private readonly Dictionary<string, RightClickObjectiveContext> _items;


        public void Initialize()
        {
            CommandSender sender = MCOS.Instance.MinecraftInstance.CommandSender;
            sender.SendCommand($"scoreboard objectives remove {ConfigManager.ScreenConfig.RightClickObjective}");
            sender.SendCommand($"scoreboard objectives add {ConfigManager.ScreenConfig.RightClickObjective} {ConfigManager.ScreenConfig.RightClickCriterion}");
        }

        public void OnTick()
        {
            PlayerList playerList = MCOS.Instance.MinecraftInstance.CommandSender.GetPlayerList();
            _items.Clear();
            foreach (var player in playerList.List)
            {
                RightClickObjectiveContext context = new(player);
                context.OnTick();
                _items.Add(player, context);
            }
        }

        public bool Query(string player)
        {
            ArgumentException.ThrowIfNullOrEmpty(player, nameof(player));

            if (_items.TryGetValue(player, out var context))
                return context.IsRightClick;
            else
                return false;
        }
    }
}
