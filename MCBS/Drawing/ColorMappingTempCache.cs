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

        public Rgba32 this[Rgba32 color] => _mapping.GetOrAdd(color, _colorFinder.Find);

        public bool IsSupportAlpha => true;
    }
}
