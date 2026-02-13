using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Drawing
{
    public class ColorBlockMapping<TPixel> : IBlockMapping<TPixel> where TPixel : unmanaged, IPixel<TPixel>
    {
        private const int COLOR_COUNT = 256 * 256 * 256;

        public ColorBlockMapping(Rgba32BlockMapping mapping, ColorMatcher<TPixel> matcher)
        {
            ArgumentNullException.ThrowIfNull(mapping, nameof(mapping));
            ArgumentNullException.ThrowIfNull(matcher, nameof(matcher));

            _mapping = mapping;
            _matcher = matcher;
        }

        private readonly Rgba32BlockMapping _mapping;

        private readonly ColorMatcher<TPixel> _matcher;

        public string this[TPixel color] => _mapping[_matcher.Match(color)];

        public TPixel this[string blockId] => new Color(_mapping[blockId]).ToPixel<TPixel>();

        public IEnumerable<TPixel> Colors => [];

        public IEnumerable<string> Blocks => _mapping.Blocks;

        public int ColorCount => COLOR_COUNT;

        public int BlockCount => _mapping.BlockCount;

        public bool ContainsColor(TPixel color)
        {
            return true;
        }

        public bool ContainsBlock(string blockId)
        {
            return _mapping.ContainsBlock(blockId);
        }

        public bool TryGetColor(string blockId, out TPixel color)
        {
            if (_mapping.TryGetColor(blockId, out var rgba32))
            {
                color = new Color(rgba32).ToPixel<TPixel>();
                return true;
            }

            color = default;
            return false;
        }

        public bool TryGetBlock(TPixel color, [MaybeNullWhen(false)] out string blockId)
        {
            blockId = _mapping[_matcher.Match(color)];
            return true;
        }
    }
}
