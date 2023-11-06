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
        public SearchMode SearchMode { get; }

        public bool SupportTransparent { get; }

        public TPixel TransparentPixel { get; }

        public OverwriteContext Overwrite(IPixelCollection<TPixel> pixels, Size size, Point location, Point offset);

        public void Fill(TPixel pixel);

        public IDictionary<Point, TPixel> GetAllPixel();

        public TPixel[] ToArray();

        public void CopyPixelDataTo(Span<TPixel> destination);
    }
}
