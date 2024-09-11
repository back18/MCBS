using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Drawing
{
    public interface IColorFinder
    {
        public Rgba32 Find(Rgba32 rgba32);

        public bool Contains(Rgba32 rgba32);
    }
}
