using QuanLib.Core;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Drawing
{
    public class ColorMappingFastCache : IColorMappingCache
    {
        public ColorMappingFastCache(Rgba32[] mapping)
        {
            ArgumentNullException.ThrowIfNull(mapping, nameof(mapping));
            ThrowHelper.ArrayLengthOutOfRange(256 * 256 * 256, mapping, nameof(mapping));

            _mapping = mapping;
        }

        private readonly Rgba32[] _mapping;

        public Rgba32 this[int index] => _mapping[index];

        public Rgba32 this[Rgba32 color] => _mapping[ToIndex(color)];

        public bool IsSupportAlpha => false;

        public static int ToIndex(Rgba32 color)
        {
            return (color.R << 16) + (color.G << 8) + color.B;
        }

        public static Rgba32 ToColor(int index)
        {
            byte r = (byte)(index >> 16 & byte.MaxValue);
            byte g = (byte)(index >> 8 & byte.MaxValue);
            byte b = (byte)(index & byte.MaxValue);
            return new(r, g, b, byte.MaxValue);
        }
    }
}
