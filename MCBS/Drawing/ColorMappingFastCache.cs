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

            _mapping = new Rgba32[256 * 256 * 256];
            new Span<Rgba32>(mapping).CopyTo(new Span<Rgba32>(_mapping));
        }

        public ColorMappingFastCache(byte[] bytes)
        {
            ArgumentNullException.ThrowIfNull(bytes, nameof(bytes));
            ThrowHelper.ArrayLengthOutOfRange(256 * 256 * 256 * 4, bytes, nameof(bytes));

            _mapping = new Rgba32[256 * 256 * 256];
            for (int i = 0; i < bytes.Length; i += 4)
                _mapping[i / 4] = new(bytes[i], bytes[i + 1], bytes[i + 2], bytes[i + 3]);
        }

        private readonly Rgba32[] _mapping;

        public Rgba32 this[int index] => _mapping[index];

        public Rgba32 this[Rgba32 color] => this[ToIndex(color)];

        public byte[] ToBytes()
        {
            int index = 0;
            byte[] bytes = new byte[_mapping.Length * 4];

            foreach (Rgba32 color in _mapping)
            {
                bytes[index++] = color.R;
                bytes[index++] = color.G;
                bytes[index++] = color.B;
                bytes[index++] = color.A;
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
    }
}
