using MCBS.Drawing;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.UI
{
    public interface IControlDrawing
    {
        public bool Visible { get; set; }

        public Point ClientLocation { get; set; }

        public Size ClientSize { get; set; }

        public Point OffsetPosition { get; set; }

        public int BorderWidth { get; set; }

        public bool IsRequestRedraw { get; }

        public void RequestRedraw();

        public DrawResult GetDrawResult();

        public BlockPixel GetForegroundColor();

        public BlockPixel GetBackgroundColor();

        public BlockPixel GetBorderColor();

        public Texture? GetBackgroundTexture();
    }
}
