using MCBS.Drawing;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.Common.Services
{
    public interface IColorMappingCacheFactory
    {
        public IColorMappingCache CreateCache(Rgba32[] mapping);
    }
}
