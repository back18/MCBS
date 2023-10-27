using QuanLib.Core;
using QuanLib.Minecraft;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Rendering
{
    public class ColorBlockFrame<TPixel> : BlockFrame<TPixel> where TPixel : unmanaged, IPixel<TPixel>
    {
        public ColorBlockFrame(int width, int height, string pixel = "", Facing facing = Facing.Zm)
        {
            ThrowHelper.ArgumentOutOfMin(0, width, nameof(width));
            ThrowHelper.ArgumentOutOfMin(0, height, nameof(height));
            if (pixel is null)
                throw new ArgumentNullException(nameof(pixel));

            _blockConverter = new(facing);
            _pixelCollection = new(width, height, _blockConverter[pixel]);
        }

        public ColorBlockFrame(Image<TPixel> image, Facing facing = Facing.Zm)
        {
            if (image is null)
                throw new ArgumentNullException(nameof(image));

            _blockConverter = new(facing);
            _pixelCollection = new(image);
        }

        private readonly ColorBlockConverter<TPixel> _blockConverter;

        private readonly ColorPixelCollection<TPixel> _pixelCollection;

        public override IBlockConverter<TPixel> BlockConverter => _blockConverter;

        public override IPixelCollection<TPixel> Pixels => _pixelCollection;

        public Facing Facing { get => _blockConverter.Facing; set => _blockConverter.Facing = value; }
    }
}
