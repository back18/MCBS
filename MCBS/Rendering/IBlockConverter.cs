using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Rendering
{
    public interface IBlockConverter<TPixel>
    {
        public string this[TPixel pixel] { get; }

        public TPixel this[string blockId] { get; }

        public IBlockMapping<TPixel> BlockMapping { get; }

        public BlockConverterMode ConverterMode { get; }
    }
}
