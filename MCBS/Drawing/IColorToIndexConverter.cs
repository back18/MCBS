using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.Drawing
{
    public interface IColorToIndexConverter
    {
        public bool IsSupportAlpha { get; }

        public int ToIndex(Rgba32 color);

        public Rgba32 ToColor(int index);
    }
}
