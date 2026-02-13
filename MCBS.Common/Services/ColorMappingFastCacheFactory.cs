using MCBS.Drawing;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.Common.Services
{
    public class ColorMappingFastCacheFactory : IColorMappingCacheFactory
    {
        public IColorMappingCache CreateCache(Rgba32[] mapping)
        {
            return new ColorMappingFastCache(mapping);
        }
    }
}
