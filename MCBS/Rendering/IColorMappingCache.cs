using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Rendering
{
    public interface IColorMappingCache
    {
        public Rgba32 this[Rgba32 color] { get; }

        public byte[] ToBytes();
    }
}
