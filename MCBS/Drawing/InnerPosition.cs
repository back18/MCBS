using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Drawing
{
    public readonly struct InnerPosition
    {
        public InnerPosition(Size baseSize, Size innerSize, Point baseLocation, Point innerLocation, int borderWidth)
        {
            OverwriteContext overwriteContext = new(baseSize, innerSize, innerSize, baseLocation, innerLocation);

            BlockFrameStartPosition = overwriteContext.BaseStartPosition;
            BlockFrameEndPosition = overwriteContext.BaseEndPosition;

            if (borderWidth > 0)
            {
                Size borderSize = new(borderWidth, borderWidth);

                BorderStartPosition = BlockFrameStartPosition - borderSize;
                BorderStartPosition.X = Math.Max(BorderStartPosition.X, 0);
                BorderStartPosition.Y = Math.Max(BorderStartPosition.Y, 0);

                BorderEndPosition = BlockFrameEndPosition + borderSize;
                BorderEndPosition.X = Math.Min(BorderEndPosition.X, baseSize.Width - 1);
                BorderEndPosition.Y = Math.Min(BorderEndPosition.Y, baseSize.Height - 1);
            }
            else
            {
                BorderStartPosition = BlockFrameStartPosition;
                BorderEndPosition = BlockFrameEndPosition;
            }
        }

        public readonly Point BlockFrameStartPosition;

        public readonly Point BlockFrameEndPosition;

        public readonly Point BorderStartPosition;

        public readonly Point BorderEndPosition;
    }
}
