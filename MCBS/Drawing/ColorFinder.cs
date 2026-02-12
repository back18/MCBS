using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Drawing
{
    public class ColorFinder : IColorFinder
    {
        public ColorFinder(IEnumerable<Rgba32> colorSet)
        {
            ArgumentNullException.ThrowIfNull(colorSet, nameof(colorSet));

            _colorSet = new HashSet<Rgba32>(colorSet);
            _colors = _colorSet.ToArray();
        }

        private readonly Rgba32[] _colors;

        private readonly HashSet<Rgba32> _colorSet;

        public int Count => _colors.Length;

        public Rgba32 Find(Rgba32 rgba32)
        {
            Rgba32 result = _colors.FirstOrDefault();
            int distance = int.MaxValue;
            foreach (var color in _colors)
            {
                int newDistance = RgbaVector.DistanceSquared(rgba32, color);
                if (distance > newDistance)
                {
                    distance = newDistance;
                    result = color;
                }
            }

            return result;
        }

        public bool Contains(Rgba32 rgba32)
        {
            return _colorSet.Contains(rgba32);
        }

        public Rgba32[] GetColorSet()
        {
            Rgba32[] colors = new Rgba32[_colors.Length];
            _colors.CopyTo(colors);
            return colors;
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
