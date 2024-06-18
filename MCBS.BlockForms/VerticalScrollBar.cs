using MCBS.Drawing;
using MCBS.Drawing.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.BlockForms
{
    public class VerticalScrollBar : ScrollBar
    {
        protected override BlockFrame Drawing()
        {
            int position = (int)Math.Round(ClientSize.Height * SliderPosition);
            int length = (int)Math.Round(ClientSize.Height * SliderSize);
            if (length < 1)
                length = 1;

            BlockFrame baseFrame = base.Drawing();
            baseFrame.Overwrite(new HashBlockFrame(ClientSize.Width, length, GetForegroundColor()), new(0, position));
            return baseFrame;
        }
    }
}
