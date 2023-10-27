using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Rendering
{
    public interface IPixelBuffer2D<TPixel> : IEnumerable<TPixel>
    {
        public TPixel this[int x, int y] { get; set; }

        public int Width { get; }

        public int Height { get; }
    }
}
