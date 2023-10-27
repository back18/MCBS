using SixLabors.ImageSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Rendering
{
    public interface IPixelCollection<TPixel> : IPixelBuffer<TPixel>, IPixelBuffer2D<TPixel>
    {
        public bool SupportTransparent { get; }

        public TPixel TransparentPixel { get; }

        public void Fill(TPixel pixel);

        public OverwriteContext Overwrite(IPixelCollection<TPixel> pixels, Point position);

        public void CopyPixelDataTo(Span<TPixel> destination);
    }
}
