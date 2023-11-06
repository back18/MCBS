using MCBS.Rendering;
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
        protected override BlockFrame Rendering()
        {
            int length = (int)Math.Round(ClientSize.Width * Progress);
            if (length <= 0)
                return base.Rendering();

            BlockFrame baseFrame = base.Rendering();
            baseFrame.Overwrite(new HashBlockFrame(length, ClientSize.Height, GetForegroundColor()), Point.Empty);
            return baseFrame;
        }
    }
}
