using SixLabors.ImageSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Drawing
{
    public interface IPixelCollection<TPixel> : IPixelBuffer<TPixel>, IPixelBuffer2D<TPixel>
    {
        public SearchMode SearchMode { get; }

        public bool SupportTransparent { get; }

        public TPixel TransparentPixel { get; }

        public bool IsTransparentPixel(int index);

        public bool IsTransparentPixel(int x, int y);

        public bool CheckTransparentPixel();

        public OverwriteContext Overwrite(IPixelCollection<TPixel> pixels, Size size, Point location, Point offset);

        public void Fill(TPixel pixel);

        public ScreenPixel<TPixel>[] GetAllPixel();

        public TPixel[] ToArray();

        public void CopyPixelDataTo(Span<TPixel> destination);
    }
}
