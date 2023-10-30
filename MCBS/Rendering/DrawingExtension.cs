﻿using SixLabors.ImageSharp;
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

        public static bool DrawVerticalLine<TPixel>(this BlockFrame<TPixel> source, int y, int start, int length, TPixel pixel)
        {
            return source.Pixels.DrawVerticalLine(y, start, length, pixel);
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
        public static OverwriteContext DrawBinary<TPixel>(this BlockFrame<TPixel> source, bool[,] binary, TPixel pixel, Point location)
        {
            return source.Pixels.DrawBinary(binary, pixel, location);
        }

        public static OverwriteContext DrawBinary<TPixel>(this IPixelBuffer2D<TPixel> source, bool[,] binary, TPixel pixel, Point location)
        {
            if (binary is null)
                throw new ArgumentNullException(nameof(binary));

            int width = binary.GetLength(0);
            int height = binary.GetLength(1);
            OverwriteContext overwriteContext = new(new(source.Width, source.Height), new(width, height), location);

            foreach (var mapping in overwriteContext)
            {
                if (binary[mapping.OverwritePosition.X, mapping.OverwritePosition.Y])
                    source[mapping.BasePosition.X, mapping.BasePosition.Y] = pixel;
            }

            return overwriteContext;
        }
    }
}