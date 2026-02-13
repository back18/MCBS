using QuanLib.Core;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Drawing
{
    public class ColorMappingCompCache : IColorMappingCache
    {
        public ColorMappingCompCache(Rgba32[] mapping)
        {
            ArgumentNullException.ThrowIfNull(mapping, nameof(mapping));
            ThrowHelper.ArrayLengthOutOfRange(256 * 256 * 256, mapping, nameof(mapping));

            Rgba32 current = mapping[0];
            List<ColorIndex> colorIndexs = [];
            colorIndexs.Add(new(0, current));

            for (int i = 0; i < mapping.Length; i++)
            {
                if (mapping[i] != current)
                {
                    current = mapping[i];
                    colorIndexs.Add(new(i, current));
                }
            }

            _colorIndexs = colorIndexs.ToArray();
        }

        private readonly ColorIndex[] _colorIndexs;

        public Rgba32 this[int index]
        {
            get
            {
                IndexRange indexRange = BinarySearch(_colorIndexs, new(0, _colorIndexs.Length - 1), index);
                return _colorIndexs[indexRange.Start].Color;
            }
        }

        public Rgba32 this[Rgba32 color] => this[ToIndex(color)];

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

        private static IndexRange BinarySearch(ColorIndex[] colorIndexs, IndexRange colorIndexRange, int index)
        {
            if (colorIndexRange.Count <= 1)
                return colorIndexRange;

            int median = colorIndexRange.Start + colorIndexRange.Count / 2;
            if (index < colorIndexs[median].StartIndex)
                return BinarySearch(colorIndexs, new(colorIndexRange.Start, median - 1), index);
            else
                return BinarySearch(colorIndexs, new(median, colorIndexRange.End), index);
        }

        private readonly struct ColorIndex(int startIndex, Rgba32 color)
        {
            public readonly int StartIndex = startIndex;

            public readonly Rgba32 Color = color;
        }
    }
}
