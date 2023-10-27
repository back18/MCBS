using QuanLib.Core;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Rendering
{
    public class HashBlockFrame : BlockFrame<int>
    {
        public HashBlockFrame(int width, int height, string pixel = "")
        {
            ThrowHelper.ArgumentOutOfMin(0, width, nameof(width));
            ThrowHelper.ArgumentOutOfMin(0, height, nameof(height));
            if (pixel is null)
                throw new ArgumentNullException(nameof(pixel));

            _blockConverter = new();
            _pixelCollection = new(width, height, _blockConverter[pixel]);
        }

        private readonly HashBlockConverter _blockConverter;

        private readonly HashPixelCollection _pixelCollection;

        public override IBlockConverter<int> BlockConverter => _blockConverter;

        public override IPixelCollection<int> Pixels => _pixelCollection;
    }
}
