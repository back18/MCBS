using MCBS.UI;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Drawing.Extensions
{
    public static class BlockFrameExtensions
    {
        public static OverwriteContext Overwrite(this BlockFrame source, BlockFrame blockFrame, Size size, Point location)
        {
            ArgumentNullException.ThrowIfNull(source, nameof(source));
            ArgumentNullException.ThrowIfNull(blockFrame, nameof(blockFrame));

            return source.Overwrite(blockFrame, size, location, Point.Empty);
        }

        public static OverwriteContext Overwrite(this BlockFrame source, BlockFrame blockFrame, Point location)
        {
            ArgumentNullException.ThrowIfNull(source, nameof(source));
            ArgumentNullException.ThrowIfNull(blockFrame, nameof(blockFrame));

            return source.Overwrite(blockFrame, new(blockFrame.Width, blockFrame.Height), location, Point.Empty);
        }
    }
}
