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
        public ColorBlockPixel(string pixel)
        {
            ArgumentNullException.ThrowIfNull(pixel, nameof(pixel));

            _blockConverter = new();
            Pixel = _blockConverter[pixel];
        }

        public ColorBlockPixel(TPixel pixel)
        {
            _blockConverter = new();
            Pixel = pixel;
        }

        private readonly ColorBlockConverter<TPixel> _blockConverter;

        public override IBlockConverter<TPixel> BlockConverter => _blockConverter;

        public override TPixel Pixel { get; }

        public Facing Facing { get => _blockConverter.Facing; set => _blockConverter.Facing = value; }

        public override BlockPixel Clone()
        {
            return new ColorBlockPixel<TPixel>(Pixel);
        }
    }
}
