using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Drawing
{
    public interface IColorMappingCache
    {
        public bool IsSupportAlpha { get; }

        public Rgba32 this[Rgba32 color] { get; }
    }
}
