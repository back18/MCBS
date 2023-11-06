using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Rendering
{
    public class ColorBlockMapping<TPixel> : IBlockMapping<TPixel> where TPixel : unmanaged, IPixel<TPixel>
    {
        public ColorBlockMapping(Rgba32BlockMapping mapping)
        {
            _mapping = mapping ?? throw new ArgumentNullException(nameof(mapping));
            _matcher = _mapping.CreateColorMatcher<TPixel>();
        }

        private readonly Rgba32BlockMapping _mapping;

        private readonly ColorMatcher<TPixel> _matcher;

        public string this[TPixel key] => _mapping[_matcher.Match(key)];

        public IEnumerable<TPixel> Keys => _matcher.GetCache().Keys;

        public IEnumerable<string> Values => _mapping.Values;

        public int Count => _mapping.Count;

        internal async Task BuildCacheAsync(TPixel[] pixels)
        {
            await _matcher.BuildCacheAsync(pixels);
        }

        public bool ContainsKey(TPixel key)
        {
            return true;
        }

        public bool TryGetValue(TPixel key, [MaybeNullWhen(false)] out string value)
        {
            value = _mapping[_matcher.Match(key)];
            return true;
        }

        public IEnumerator<KeyValuePair<TPixel, string>> GetEnumerator()
        {
            foreach (var item in _mapping)
            {
                yield return new(new Color(item.Key).ToPixel<TPixel>(), item.Value);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
