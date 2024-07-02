using MCBS.UI;
using QuanLib.Game;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
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

        public static Image<Rgba32> ToImage(this BlockFrame source, Facing facing)
        {
            ArgumentNullException.ThrowIfNull(source, nameof(source));

            Image<Rgba32> image = new(source.Width, source.Width, default);
            for (int y = 0; y < source.Height; y++)
                for (int x = 0; x < source.Width; x++)
                {
                    string blockId = source[x, y];

                    if (string.IsNullOrEmpty(blockId))
                        continue;

                    if (!SR.Rgba32BlockMappings[facing].TryGetKey(blockId, out var rgba32))
                        continue;

                    image[x, y] = rgba32;
                }

            return image;
        }
    }
}
