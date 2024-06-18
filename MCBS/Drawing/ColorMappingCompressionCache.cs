using QuanLib.Core;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Drawing
{
    public class ColorMappingCompressionCache : IColorMappingCache
    {
        public ColorMappingCompressionCache(Rgba32[] mapping)
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

        public ColorMappingCompressionCache(byte[] bytes)
        {
            ArgumentNullException.ThrowIfNull(bytes, nameof(bytes));
            if (bytes.Length % 8 != 0)
                throw new ArgumentException("数据格式不合法", nameof(bytes));

            _colorIndexs = new ColorIndex[bytes.Length / 8];
            for (int i = 0; i < bytes.Length; i += 8)
            {
                int startIndex = BitConverter.ToInt32(bytes, i);
                Rgba32 color = new(bytes[i + 4], bytes[i + 5], bytes[i + 6], bytes[i + 7]);
                _colorIndexs[i / 8] = new(startIndex, color);
            }
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

        public byte[] ToBytes()
        {
            int index = 0;
            byte[] bytes = new byte[_colorIndexs.Length * 8];

            foreach (ColorIndex colorIndex in _colorIndexs)
            {
                byte[] indexBytes = BitConverter.GetBytes(colorIndex.StartIndex);
                bytes[index++] = indexBytes[0];
                bytes[index++] = indexBytes[1];
                bytes[index++] = indexBytes[2];
                bytes[index++] = indexBytes[3];
                bytes[index++] = colorIndex.Color.R;
                bytes[index++] = colorIndex.Color.G;
                bytes[index++] = colorIndex.Color.B;
                bytes[index++] = colorIndex.Color.A;
            }

            return bytes;
        }

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
