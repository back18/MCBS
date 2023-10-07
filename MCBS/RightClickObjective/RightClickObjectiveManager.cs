using QuanLib.Minecraft;
using QuanLib.Minecraft.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.RightClickObjective
{
    public class RightClickObjectiveManager
    {
        public RightClickObjectiveManager()
        {
            _items = new();
        }

        private readonly Dictionary<string, RightClickObjectiveContext> _items;

        public void RightClickObjectiveScheduling()
        {
            PlayerList playerList = MCOS.Instance.MinecraftInstance.CommandSender.GetPlayerList();
            _items.Clear();
            foreach (var player in playerList.List)
            {
                RightClickObjectiveContext context = new(player);
                context.Handle();
                _items.Add(player, context);
            }
        }

        public bool Query(string player)
        {
            if (string.IsNullOrEmpty(player))
                throw new ArgumentException($"“{nameof(player)}”不能为 null 或空。", nameof(player));

            if (_items.TryGetValue(player, out var context))
                return context.IsRightClick;
            else
                return false;
        }
    }
}
