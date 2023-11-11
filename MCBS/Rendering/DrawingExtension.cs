using MCBS.UI;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Rendering
{
    public static class DrawingExtension
    {
        public static bool DrawHorizontalLine<TPixel>(this BlockFrame<TPixel> source, int y, int start, int length, TPixel pixel)
        {
            return source.Pixels.DrawHorizontalLine(y, start, length, pixel);
        }

        public static bool DrawHorizontalLine<TPixel>(this IPixelBuffer2D<TPixel> source, int y, int start, int length, TPixel pixel)
        {
            if (y < 0 || y > source.Height - 1)
                return false;

            if (start < 0)
            {
                length += start;
                start = 0;
            }

            if (length <= 0)
                return false;

            int end = start + length  - 1;
            if (end > source.Width - 1)
                end = source.Width - 1;

            for (int x = start; x <= end; x++)
                source[x, y] = pixel;

            return true;
        }

        public static bool DrawHorizontalLine<TPixel>(this BlockFrame<TPixel> source, int y, TPixel pixel)
        {
            return source.DrawHorizontalLine(y, 0, source.Width, pixel);
        }

        public static bool DrawHorizontalLine<TPixel>(this IPixelBuffer2D<TPixel> source, int y, TPixel pixel)
        {
            return source.DrawHorizontalLine(y, 0, source.Width, pixel);
        }

        public static bool DrawVerticalLine<TPixel>(this BlockFrame<TPixel> source, int x, int start, int length, TPixel pixel)
        {
            return source.Pixels.DrawVerticalLine(x, start, length, pixel);
        }

        public static bool DrawVerticalLine<TPixel>(this IPixelBuffer2D<TPixel> source, int x, int start, int length, TPixel pixel)
        {
            if (x < 0 || x > source.Width - 1)
                return false;

            if (start < 0)
            {
                length += start;
                start = 0;
            }

            if (length <= 0)
                return false;

            int end = start + length - 1;
            if (end > source.Height - 1)
                end = source.Height - 1;

            for (int y = start; y <= end; y++)
                source[x, y] = pixel;

            return true;
        }

        public static bool DrawVerticalLine<TPixel>(this BlockFrame<TPixel> source, int y, TPixel pixel)
        {
            return source.DrawVerticalLine(y, 0, source.Height, pixel);
        }

        public static bool DrawVerticalLine<TPixel>(this IPixelBuffer2D<TPixel> source, int x, TPixel pixel)
        {
            return source.DrawVerticalLine(x, 0, source.Height, pixel);
        }

        public static OverwriteContext DrawBinary<TPixel>(this BlockFrame<TPixel> source, bool[,] binary, TPixel pixel, Point location)
        {
            return source.Pixels.DrawBinary(binary, pixel, location);
        }

        public static OverwriteContext DrawBinary<TPixel>(this IPixelBuffer2D<TPixel> source, bool[,] binary, TPixel pixel, Point location)
        {
            if (binary is null)
                throw new ArgumentNullException(nameof(binary));

            Size size = new(binary.GetLength(0), binary.GetLength(1));
            OverwriteContext overwriteContext = new(new(source.Width, source.Height), size, size, location, Point.Empty);

            foreach (var mapping in overwriteContext)
            {
                if (binary[mapping.OverwritePosition.X, mapping.OverwritePosition.Y])
                    source[mapping.BasePosition.X, mapping.BasePosition.Y] = pixel;
            }

            return overwriteContext;
        }

        public static void DrawBorder(this BlockFrame source, IControlRendering rendering, Point location)
        {
            if (rendering.BorderWidth > 0)
            {
                int width = rendering.ClientSize.Width + rendering.BorderWidth * 2;
                int heigth = rendering.ClientSize.Height + rendering.BorderWidth * 2;

                int startTop = location.Y - 1;
                int startBottom = location.Y + rendering.ClientSize.Height;
                int startLeft = location.X - 1;
                int startRigth = location.X + rendering.ClientSize.Width;
                int endTop = location.Y - rendering.BorderWidth;
                int endBottom = location.Y + rendering.ClientSize.Height + rendering.BorderWidth - 1;
                int endLeft = location.X - rendering.BorderWidth;
                int endRight = location.X + rendering.ClientSize.Width + rendering.BorderWidth - 1;

                string blockID = rendering.GetBorderColor().ToBlockId();

                for (int y = startTop; y >= endTop; y--)
                    source.DrawHorizontalLine(y, endLeft, width, blockID);
                for (int y = startBottom; y <= endBottom; y++)
                    source.DrawHorizontalLine(y, endLeft, width, blockID);
                for (int x = startLeft; x >= endLeft; x--)
                    source.DrawVerticalLine(x, endTop, heigth, blockID);
                for (int x = startRigth; x <= endRight; x++)
                    source.DrawVerticalLine(x, endTop, heigth, blockID);
            }
        }

        public static void DrawBorder(this BlockFrame source, IControlRendering rendering, Point location, Point offset)
        {
            location = new(location.X - offset.X, location.Y - offset.Y);
            source.DrawBorder(rendering, location);
        }
    }
}
