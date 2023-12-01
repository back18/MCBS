using MCBS.UI;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Rendering
{
    public static class BlockFrameExtension
    {
        public static OverwriteContext Overwrite(this BlockFrame source, BlockFrame blockFrame, Size size, Point location)
        {
            ArgumentNullException.ThrowIfNull(blockFrame, nameof(blockFrame));

            return source.Overwrite(blockFrame, size, location, Point.Empty);
        }

        public static OverwriteContext Overwrite(this BlockFrame source, BlockFrame blockFrame, Point location)
        {
            ArgumentNullException.ThrowIfNull(blockFrame, nameof(blockFrame));

            return source.Overwrite(blockFrame, new(blockFrame.Width, blockFrame.Height), location, Point.Empty);
        }

        //public static OverwriteContext Overwrite(this BlockFrame source, BlockFrame blockFrame, Point location, Point offset)
        //{
        //    if (blockFrame is null)
        //        throw new ArgumentNullException(nameof(blockFrame));

        //    location = new(location.X - offset.X, location.Y - offset.Y);
        //    return source.Overwrite(blockFrame, new(blockFrame.Width, blockFrame.Height), location, Point.Empty);
        //}

        //public static OverwriteContext Overwrite(this BlockFrame source, BlockFrame blockFrame, Point location, Point offset, Size size)
        //{
        //    if (blockFrame is null)
        //        throw new ArgumentNullException(nameof(blockFrame));

        //    location = new(location.X - offset.X, location.Y - offset.Y);
        //    return source.Overwrite(blockFrame, location, size);
        //}
    }
}
