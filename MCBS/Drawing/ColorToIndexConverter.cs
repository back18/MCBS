using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.Drawing
{
    public class ColorToIndexConverter : IColorToIndexConverter
    {
        public bool IsSupportAlpha => false;

        public int ToIndex(Rgba32 color)
        {
            return (color.R << 16) + (color.G << 8) + color.B;
        }

        public Rgba32 ToColor(int index)
        {
            byte r = (byte)(index >> 16 & byte.MaxValue);
            byte g = (byte)(index >> 8 & byte.MaxValue);
            byte b = (byte)(index & byte.MaxValue);
            return new(r, g, b, byte.MaxValue);
        }
    }
}
