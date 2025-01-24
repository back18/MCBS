using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Drawing
{
    public class ColorMatcher<TPixel> where TPixel : unmanaged, IPixel<TPixel>
    {
        public ColorMatcher(IColorFinder colorFinder, IColorMappingCache mappingCache)
        {
            ArgumentNullException.ThrowIfNull(colorFinder, nameof(colorFinder));
            ArgumentNullException.ThrowIfNull(mappingCache, nameof(mappingCache));

            _colorFinder = colorFinder;
            _mappingCache = mappingCache;
        }

        private readonly IColorFinder _colorFinder;

        private readonly IColorMappingCache _mappingCache;

        public Rgba32 Match(TPixel pixel)
        {
            Rgba32 rgba32 = ToRgba32(pixel);
            if (rgba32.A == byte.MaxValue || _mappingCache.IsSupportAlpha)
                return _mappingCache[rgba32];
            else if (rgba32 == default || _colorFinder.Contains(rgba32))
                return rgba32;
            else
                return _colorFinder.Find(rgba32);
        }

        public async Task<Rgba32[]> MatchAsync(TPixel[] pixels)
        {
            ArgumentNullException.ThrowIfNull(pixels, nameof(pixels));

            return await Task.Run(() =>
            {
                ColorMappingTempCache colorMapping = new(_colorFinder);
                Rgba32[] result = new Rgba32[pixels.Length];

                for (int i = 0; i < pixels.Length; i++)
                {
                    Rgba32 rgba32 = ToRgba32(pixels[i]);
                    if (rgba32.A == byte.MaxValue || _mappingCache.IsSupportAlpha)
                        result[i] = _mappingCache[rgba32];
                    else if (_colorFinder.Contains(rgba32))
                        result[i] = rgba32;
                    else
                        result[i] = colorMapping[rgba32];
                }

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
                Image<Rgba32> result = new(image.Width, image.Height);
                int index = 0;

                for (int y = 0; y < result.Width; y++)
                    for (int x = 0; x < result.Height; x++)
                        result[x, y] = matchs[index++];

                return result;
            });
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

        private static Rgba32 ToRgba32(TPixel pixel)
        {
            if (pixel is not Rgba32 rgba32)
            {
                rgba32 = default;
                pixel.ToRgba32(ref rgba32);
            }

            return rgba32;
        }
    }
}
