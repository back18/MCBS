using QuanLib.Minecraft.Command;
using QuanLib.Minecraft.Command.Senders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Cursor
{
    public class ClickReader
    {
        public ClickReader(string playerName)
        {
            if (string.IsNullOrEmpty(playerName))
                throw new ArgumentException($"“{nameof(playerName)}”不能为 null 或空。", nameof(playerName));

            PlayerName = playerName;
            LeftClickTime = DateTime.MinValue;
            RightClickTime = DateTime.MinValue;
        }

        public string PlayerName { get; }

        public DateTime LeftClickTime { get; internal set; }

        public DateTime RightClickTime { get; internal set; }

        public ClickResult ReadClick()
        {
            bool isLeftClick = false;
            bool isRightClick = false;
            CommandSender sender = MCOS.Instance.MinecraftInstance.CommandSender;
            DateTime now = DateTime.Now;
            if (sender.TryGetEntityUuid(PlayerName, out var uuid) && MCOS.Instance.InteractionManager.Items.TryGetValue(uuid, out var interaction))
            {
                if (interaction.IsLeftClick)
                {
                    isLeftClick = true;
                    LeftClickTime = now;
                }
                if (interaction.IsRightClick)
                {
                    isRightClick = true;
                    RightClickTime = now;
                }
            }
            else
            {
                if (MCOS.Instance.RightClickObjectiveManager.Query(PlayerName))
                {
                    isRightClick = true;
                    RightClickTime = now;
                }
            }

            return new(isLeftClick, isRightClick);
        }
    }
}
