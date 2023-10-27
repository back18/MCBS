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
        public static OverwriteContext Overwrite(this BlockFrame source, BlockFrame blockFrame, Point location, Point offset)
        {
            location = new(location.X - offset.X, location.Y - offset.Y);
            return source.Overwrite(blockFrame, location);
        }
    }
}
