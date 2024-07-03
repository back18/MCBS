using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Drawing
{
    public readonly struct ScreenPixel<T>(Point position, T pixel)
    {
        public readonly Point Position = position;

        public readonly T Pixel = pixel;
    }
}
