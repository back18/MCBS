using MCBS.Rendering;
using MCBS.Rendering.Extensions;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.BlockForms
{
    public class VerticalProgressBar : ProgressBar
    {
        protected override BlockFrame Rendering()
        {
            int length = (int)Math.Round(ClientSize.Height * Progress);
            if (length <= 0)
                return base.Rendering();

            BlockFrame baseFrame = base.Rendering();
            baseFrame.Overwrite(new HashBlockFrame(ClientSize.Width, length, GetForegroundColor()), Point.Empty);
            return baseFrame;
        }
    }
}
