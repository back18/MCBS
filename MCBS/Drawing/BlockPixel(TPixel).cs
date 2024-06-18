using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Drawing
{
    public abstract class BlockPixel<TPixel> : BlockPixel
    {
        public abstract IBlockConverter<TPixel> BlockConverter { get; }

        public abstract TPixel Pixel { get; }

        public override string ToBlockId()
        {
            return BlockConverter[Pixel];
        }
    }
}
