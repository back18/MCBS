using MCBS.Events;
using MCBS.UI;
using QuanLib.Minecraft.Blocks;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.BlockForms
{
    public class Button : TextControl, IButton
    {
        public Button()
        {
            ReboundTime = 10;
            ReboundCountdown = 0;

            Skin.BackgroundBlockID = BlockManager.Concrete.LightBlue;
            Skin.BackgroundBlockID_Hover = BlockManager.Concrete.Yellow;
            Skin.BackgroundBlockID_Selected = BlockManager.Concrete.Lime;
            Skin.BackgroundBlockID_Hover_Selected = BlockManager.Concrete.Lime;
            ContentAnchor = AnchorPosition.Centered;
        }

        public int ReboundTime { get; set; }

        public int ReboundCountdown { get; private set; }

        protected override void OnRightClick(Control sender, CursorEventArgs e)
        {
            base.OnRightClick(sender, e);

            if (!IsSelected)
            {
                IsSelected = true;
                ReboundCountdown = ReboundTime;
            }
        }

        protected override void OnBeforeFrame(Control sender, EventArgs e)
        {
            base.OnBeforeFrame(sender, e);

            if (IsSelected)
            {
                if (ReboundCountdown <= 0)
                    IsSelected = false;
                ReboundCountdown--;
            }
        }
    }
}
