using MCBS.Events;
using MCBS.Rendering;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.UI
{
    public interface IControlRendering
    {
        public bool Visible { get; set; }

        public Point ClientLocation { get; set; }

        public Size ClientSize { get; set; }

        public Point OffsetPosition { get; set; }

        public int BorderWidth { get; set; }

        public Task<BlockFrame> GetRenderingResultAsync();

        public BlockPixel GetForegroundColor();

        public BlockPixel GetBackgroundColor();

        public BlockPixel GetBorderColor();

        public Texture? GetBackgroundTexture();
    }
}
