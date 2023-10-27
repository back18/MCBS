using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Rendering
{
    public class HashBlockPixel : BlockPixel<int>
    {
        public HashBlockPixel(int pixel)
        {
            Pixel = pixel;
            _blockConverter = new();
        }

        private readonly HashBlockConverter _blockConverter;

        public override int Pixel { get; set; }

        public override IBlockConverter<int> BlockConverter => _blockConverter;
    }
}
