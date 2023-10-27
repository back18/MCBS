using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Rendering
{
    public class HashBlockConverter : IBlockConverter<int>
    {
        public HashBlockConverter()
        {
            _mapping = SR.HashBlockMapping;
        }

        private readonly HashBlockMapping _mapping;

        public string this[int pixel]
        {
            get
            {
                if (_mapping.TryGetValue(pixel, out var blockId))
                    return blockId;
                else
                    return string.Empty;
            }
        }

        public int this[string blockId] => _mapping.RegistrationHash(blockId);

        public IBlockMapping<int> BlockMapping => _mapping;

        public BlockConverterMode ConverterMode => BlockConverterMode.ToPixel;
    }
}
