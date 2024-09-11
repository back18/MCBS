using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Drawing
{
    public class ColorMappingTempCache : IColorMappingCache
    {
        public ColorMappingTempCache(IColorFinder colorFinder)
        {
            ArgumentNullException.ThrowIfNull(colorFinder, nameof(colorFinder));

            _colorFinder = colorFinder;
            _mapping = [];
        }

        private readonly IColorFinder _colorFinder;

        private readonly ConcurrentDictionary<Rgba32, Rgba32> _mapping;

        public Rgba32 this[Rgba32 color]
        {
            get
            {
                if (!_mapping.TryGetValue(color, out var result))
                {
                    result = _colorFinder.Find(color);
                    _mapping[color] = result;
                }

                return result;
            }
        }

        public bool IsSupportAlpha => true;

        public byte[] ToBytes()
        {
            throw new NotSupportedException();
        }

        private readonly struct RgbaVector(int r, int g, int b, int a)
        {
            public RgbaVector(Rgba32 rgba32) : this(rgba32.R, rgba32.G, rgba32.B, rgba32.A) { }

            public readonly int R = r;

            public readonly int G = g;

            public readonly int B = b;

            public readonly int A = a;

            public static int DistanceSquared(Rgba32 value1, Rgba32 value2)
            {
                RgbaVector vector1 = new(value1);
                RgbaVector vector2 = new(value2);
                RgbaVector difference = new(
                    vector1.R - vector2.R,
                    vector1.G - vector2.G,
                    vector1.B - vector2.B,
                    vector1.A - vector2.A);
                return difference.R * difference.R
                     + difference.G * difference.G
                     + difference.B * difference.B
                     + difference.A * difference.A;
            }
        }
    }
}
