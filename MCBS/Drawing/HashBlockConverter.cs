using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Drawing
{
    public class HashBlockConverter : IBlockConverter<int>
    {
        public HashBlockConverter()
        {
            _mapping = MinecraftResourceManager.HashBlockMapping;
        }

        private readonly HashBlockMapping _mapping;

        public string this[int pixel]
        {
            get
            {
                if (_mapping.TryGetBlock(pixel, out var blockId))
                    return blockId;
                else
                    return string.Empty;
            }
        }

        public int this[string blockId] => _mapping.TryGetColor(blockId, out var hash) ? hash : blockId.GetHashCode();

        public IBlockMapping<int> BlockMapping => _mapping;

        public BlockConverterMode ConverterMode => BlockConverterMode.ToPixel;
    }
}
