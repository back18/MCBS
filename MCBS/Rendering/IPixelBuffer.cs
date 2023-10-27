using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Rendering
{
    public interface IPixelBuffer<TPixel> : IReadOnlyCollection<TPixel>
    {
        public TPixel this[int index] { get; set; }
    }
}
