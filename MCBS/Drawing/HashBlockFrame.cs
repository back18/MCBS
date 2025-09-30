using QuanLib.Core;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Drawing
{
    public class HashBlockFrame : BlockFrame<int>
    {
        public HashBlockFrame(int width, int height, string pixel = "")
        {
            ThrowHelper.ArgumentOutOfMin(1, width, nameof(width));
            ThrowHelper.ArgumentOutOfMin(1, height, nameof(height));
            ArgumentNullException.ThrowIfNull(pixel, nameof(pixel));

            _blockConverter = new();
            _pixelCollection = new(width, height, _blockConverter[pixel]);
            ContainsTransparent = string.IsNullOrEmpty(pixel);
        }

        private HashBlockFrame(HashPixelCollection pixelCollection)
        {
            ArgumentNullException.ThrowIfNull(pixelCollection, nameof(pixelCollection));

            _blockConverter = new();
            _pixelCollection = pixelCollection;
        }

        public HashBlockFrame(int width, int height, BlockPixel pixel) : this(width, height, pixel?.ToBlockId() ?? throw new ArgumentNullException(nameof(pixel))) { }

        public HashBlockFrame(Size size, string pixel = "") : this(size.Width, size.Height, pixel) { }

        public HashBlockFrame(Size size, BlockPixel pixel) : this(size.Width, size.Height, pixel) { }

        private readonly HashBlockConverter _blockConverter;

        private readonly HashPixelCollection _pixelCollection;

        public override IBlockConverter<int> BlockConverter => _blockConverter;

        public override IPixelCollection<int> Pixels => _pixelCollection;

        public override BlockFrame Crop(Rectangle rectangle)
        {
            return new HashBlockFrame(_pixelCollection.Crop(rectangle));
        }

        public override BlockFrame Clone()
        {
            return new HashBlockFrame(_pixelCollection.Clone());
        }
    }
}
