using SixLabors.ImageSharp;
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
    public class ColorBlockMapping<TPixel> : IBlockMapping<TPixel> where TPixel : unmanaged, IPixel<TPixel>
    {
        public ColorBlockMapping(Rgba32BlockMapping mapping)
        {
            ArgumentNullException.ThrowIfNull(mapping, nameof(mapping));

            _mapping = mapping;
            _matcher = _mapping.CreateColorMatcher<TPixel>();
        }

        private readonly Rgba32BlockMapping _mapping;

        private readonly ColorMatcher<TPixel> _matcher;

        public string this[TPixel key] => _mapping[_matcher.Match(key)];

        public TPixel this[string value] => new Color(_mapping[value]).ToPixel<TPixel>();

        public IEnumerable<TPixel> Keys => [];

        public IEnumerable<string> Values => _mapping.Values;

        public int Count => _mapping.Count;

        public bool ContainsKey(TPixel key)
        {
            return true;
        }

        public bool TryGetValue(TPixel key, [MaybeNullWhen(false)] out string value)
        {
            value = _mapping[_matcher.Match(key)];
            return true;
        }

        public bool TryGetKey(string value, out TPixel key)
        {
            if (_mapping.TryGetKey(value, out var rgba32))
            {
                key = new Color(rgba32).ToPixel<TPixel>();
                return true;
            }

            key = default;
            return false;
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
