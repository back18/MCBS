using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Rendering
{
    public class ColorMatcher<TPixel> where TPixel : unmanaged, IPixel<TPixel>
    {
        public ColorMatcher(IEnumerable<Rgba32> colors)
        {
            _colors = colors ?? throw new ArgumentNullException(nameof(colors));
            _cache = new();
        }

        private readonly IEnumerable<Rgba32> _colors;

        private readonly Dictionary<TPixel, Rgba32> _cache;

        private Rgba32 Match(Rgba32 rgba32)
        {
            Rgba32 result = _colors.FirstOrDefault();
            int distance = 0;
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

        public Rgba32 Match(TPixel pixel)
        {
            if (_cache.TryGetValue(pixel, out var result))
                return result;

            if (pixel is not Rgba32 rgba32)
            {
                rgba32 = new();
                pixel.ToRgba32(ref rgba32);
            }

            result = Match(rgba32);

            if (!_cache.ContainsKey(pixel))
            {
                lock (_cache)
                    _cache.TryAdd(pixel, result);
            }

            return result;
        }

        public Rgba32[] Match(TPixel[] pixels)
        {
            if (pixels is null)
                throw new ArgumentNullException(nameof(pixels));

            HashSet<TPixel> matchs = new();
            foreach (TPixel pixel in pixels)
            {
                if (!matchs.Contains(pixel) && !_cache.ContainsKey(pixel))
                    matchs.Add(pixel);
            }

            int count = 0;
            Parallel.ForEach(matchs, match =>
            {
                Match(match);
                Interlocked.Increment(ref count);
            });

            if (count < matchs.Count)
                Thread.Yield();

            Rgba32[] result = new Rgba32[pixels.Length];
            for (int i = 0; i < pixels.Length; i++)
                result[i] = _cache[pixels[i]];

            return result;
        }

        public Image<Rgba32> Match(Image<TPixel> image)
        {
            if (image is null)
                throw new ArgumentNullException(nameof(image));

            TPixel[] pixels = new TPixel[image.Width * image.Height];
            Span<TPixel> span = new(pixels);
            image.CopyPixelDataTo(span);
            Rgba32[] matchs = Match(pixels);

            int index = 0;
            Image<Rgba32> result = new(image.Width, image.Height);
            for (int y = 0; y < result.Width; y++)
            {
                for (int x = 0; x < result.Height; x++)
                {
                    result[x, y] = matchs[index++];
                }
            }

            return result;
        }

        internal IReadOnlyDictionary<TPixel, Rgba32> GetCache()
        {
            return _cache;
        }

        private class RgbaVector
        {
            public RgbaVector(int r, int g, int b, int a)
            {
                R = r;
                G = g;
                B = b;
                A = a;
            }

            public RgbaVector(Rgba32 rgba32)
            {
                R = rgba32.R;
                G = rgba32.G;
                B = rgba32.B;
                A = rgba32.A;
            }

            public int R;

            public int G;

            public int B;

            public int A;

            public static int DistanceSquared(Rgba32 value1, Rgba32 value2)
            {
                RgbaVector vector1 = new(value1);
                RgbaVector vector2 = new(value2);
                RgbaVector difference = new(
                    vector1.R - vector2.R,
                    vector1.G - vector2.G,
                    vector1.B - vector2.B,
                    vector1.A - vector2.A);
                return (difference.R * difference.R)
                     + (difference.G * difference.G)
                     + (difference.B * difference.B)
                     + (difference.A * difference.A);
            }
        }
    }
}
