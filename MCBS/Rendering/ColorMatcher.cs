using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Rendering
{
    public class ColorMatcher<TPixel> where TPixel : unmanaged, IPixel<TPixel>
    {
        public ColorMatcher(ISet<Rgba32> colors, ColorMappingCache? mappingCache = null)
        {
            ArgumentNullException.ThrowIfNull(colors, nameof(colors));

            _colors = colors.ToArray();
            _mappingCache = mappingCache;
            _tempCache = new();
        }

        private readonly Rgba32[] _colors;

        private readonly ColorMappingCache? _mappingCache;

        private readonly ConcurrentDictionary<TPixel, Rgba32> _tempCache;

        private Rgba32 Match(Rgba32 rgba32)
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

        public Rgba32 Match(TPixel pixel)
        {
            if (_tempCache.TryGetValue(pixel, out var result))
                return result;

            if (pixel is not Rgba32 rgba32)
            {
                rgba32 = default;
                pixel.ToRgba32(ref rgba32);
            }

            if (_mappingCache is not null && rgba32.A == byte.MaxValue)
                return _mappingCache[rgba32];

            result = Match(rgba32);
            _tempCache.TryAdd(pixel, result);
            return result;
        }

        public async Task<Rgba32[]> MatchAsync(TPixel[] pixels)
        {
            await BuildCacheAsync(pixels);

            return await Task.Run(() =>
            {
                Rgba32[] result = new Rgba32[pixels.Length];
                for (int i = 0; i < pixels.Length; i++)
                    result[i] = _tempCache[pixels[i]];
                return result;
            });
        }

        public async Task<Image<Rgba32>> MatchAsync(Image<TPixel> image)
        {
            ArgumentNullException.ThrowIfNull(image, nameof(image));

            TPixel[] pixels = await GetPixelsAsync(image);
            Rgba32[] matchs = await MatchAsync(pixels);

            return await Task.Run(() =>
            {
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
            });
        }

        public async Task BuildCacheAsync(TPixel[] pixels)
        {
            ArgumentNullException.ThrowIfNull(pixels, nameof(pixels));

            HashSet<TPixel> matchs = new();
            await Task.Run(() =>
            {
                foreach (TPixel pixel in pixels)
                {
                    if (!matchs.Contains(pixel) && !_tempCache.ContainsKey(pixel))
                        matchs.Add(pixel);
                }

                ParallelLoopResult parallelLoopResult = Parallel.ForEach(matchs, match =>
                {
                    Match(match);
                });

                if (!parallelLoopResult.IsCompleted)
                     Thread.Yield();
            });
        }

        public async Task BuildCacheAsync(Image<TPixel> image)
        {
            ArgumentNullException.ThrowIfNull(image, nameof(image));

            TPixel[] pixels = await GetPixelsAsync(image);
            await BuildCacheAsync(pixels);
        }

        private static async Task<TPixel[]> GetPixelsAsync(Image<TPixel> image)
        {
            ArgumentNullException.ThrowIfNull(image, nameof(image));

            return await Task.Run(() =>
            {
                TPixel[] pixels = new TPixel[image.Width * image.Height];
                Span<TPixel> span = new(pixels);
                image.CopyPixelDataTo(span);
                return pixels;
            });
        }

        internal IReadOnlyDictionary<TPixel, Rgba32> GetCache()
        {
            return _tempCache;
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
