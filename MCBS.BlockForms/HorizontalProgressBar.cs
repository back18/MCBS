using MCBS.Drawing;
using MCBS.Drawing.Extensions;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.BlockForms
{
    public class HorizontalProgressBar : ProgressBar
    {
        protected override BlockFrame Drawing()
        {
            int length = (int)Math.Round(ClientSize.Width * Progress);
            if (length <= 0)
                return base.Drawing();

            BlockFrame baseFrame = base.Drawing();
            baseFrame.Overwrite(new HashBlockFrame(length, ClientSize.Height, GetForegroundColor()), Point.Empty);
            return baseFrame;
        }
    }
}
