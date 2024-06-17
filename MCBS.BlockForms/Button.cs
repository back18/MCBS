﻿using MCBS.BlockForms.Utility;
using MCBS.Events;
using MCBS.UI;
using QuanLib.Minecraft.Blocks;
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

            Skin.SetBackgroundColor(BlockManager.Concrete.LightBlue, ControlState.None);
            Skin.SetBackgroundColor(BlockManager.Concrete.Yellow, ControlState.Hover);
            Skin.SetBackgroundColor(BlockManager.Concrete.Lime, ControlState.Selected, ControlState.Hover | ControlState.Selected);
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
