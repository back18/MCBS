using QuanLib.Minecraft;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Rendering
{
    public class ColorBlockPixel<TPixel> : BlockPixel<TPixel> where TPixel : unmanaged, IPixel<TPixel>
    {
        public ColorBlockPixel(TPixel pixel)
        {
            Pixel = pixel;
            _blockConverter = new();
        }

        private readonly ColorBlockConverter<TPixel> _blockConverter;

        public override IBlockConverter<TPixel> BlockConverter => _blockConverter;

        public override TPixel Pixel { get; set; }

        public Facing Facing { get => _blockConverter.Facing; set => _blockConverter.Facing = value; }
    }
}
