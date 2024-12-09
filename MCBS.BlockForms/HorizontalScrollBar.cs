using MCBS.Drawing;
using MCBS.Drawing.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.BlockForms
{
    public class HorizontalScrollBar : ScrollBar
    {
        public HorizontalScrollBar()
        {
            FirstHandleRightClick = true;

            Skin.SetBorderColor(string.Empty, Utility.ControlState.None, Utility.ControlState.Selected);
            Skin.SetBackgroundColor(string.Empty, Utility.ControlState.None, Utility.ControlState.Selected);
        }

        protected override BlockFrame Drawing()
        {
            int position = (int)Math.Round(ClientSize.Width * SliderPosition);
            int length = (int)Math.Round(ClientSize.Width * SliderSize);
            if (length < 1)
                length = 1;

            BlockFrame baseFrame = base.Drawing();

            if (ControlState.HasFlag(Utility.ControlState.Hover))
                baseFrame.Overwrite(new HashBlockFrame(length, ClientSize.Height, GetForegroundColor()), new(position, 0));
            else
                baseFrame.DrawHorizontalLine(Height / 2, position, length, GetForegroundColor().ToBlockId());

            return baseFrame;
        }
    }
}
