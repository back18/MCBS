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
            ArgumentException.ThrowIfNullOrEmpty(playerName, nameof(playerName));

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
            DateTime now = DateTime.Now;
            if ( MinecraftBlockScreen.Instance.InteractionManager.Items.TryGetValue(PlayerName, out var interaction))
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
                if (MinecraftBlockScreen.Instance.ScoreboardManager.IsRightClick(PlayerName))
                {
                    isRightClick = true;
                    RightClickTime = now;
                }
            }

            return new(isLeftClick, isRightClick);
        }
    }
}
