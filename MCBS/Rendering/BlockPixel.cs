using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Rendering
{
    public abstract class BlockPixel<TPixel>
    {
        public abstract IBlockConverter<TPixel> BlockConverter { get; }

        public abstract TPixel Pixel { get; set; }

        public virtual string ToSrring()
        {
            return BlockConverter[Pixel];
        }
    }
}
