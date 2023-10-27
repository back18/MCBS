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
    public class ColorBlockConverter<TPixel> : IBlockConverter<TPixel> where TPixel : unmanaged, IPixel<TPixel>
    {
        public ColorBlockConverter(Facing facing = Facing.Zm)
        {
            _Facing = facing;
            _mapping = new(SR.Rgba32BlockMappings[facing]);
        }

        private ColorBlockMapping<TPixel> _mapping;

        public Facing Facing
        {
            get => _Facing;
            set
            {
                if (_Facing != value)
                {
                    _Facing = value;
                    _mapping = new(SR.Rgba32BlockMappings[value]);
                }
            }
        }
        private Facing _Facing;

        public string this[TPixel pixel] => _mapping[pixel];

        public TPixel this[string blockId]
        {
            get
            {
                if (SR.BlockTextureManager.TryGetValue(blockId, out var texture))
                    return new Color(texture.Textures[Facing].AverageColor).ToPixel<TPixel>();
                else
                    return default;
            }
        }

        public IBlockMapping<TPixel> BlockMapping => _mapping;

        public BlockConverterMode ConverterMode => BlockConverterMode.ToBlockId;
    }
}
