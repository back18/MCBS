using QuanLib.Game;
using QuanLib.Minecraft;
using QuanLib.Minecraft.ResourcePack.Block;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Drawing
{
    public class Rgba32BlockMapping : IBlockMapping<Rgba32>
    {
        public Rgba32BlockMapping(Facing facing, BlockTexture[] blockTextures)
        {
            ArgumentNullException.ThrowIfNull(blockTextures, nameof(blockTextures));

            Facing = facing;
            _color2Block = [];
            _block2Color = [];

            foreach (BlockTexture blockTexture in blockTextures)
            {
                if (!BlockState.TryParse(blockTexture.BlockId, out var blockState))
                    continue;

                Rgba32 color = blockTexture.Textures[facing].AverageColor;
                string blockId = blockTexture.BlockId;

                if (blockTexture.BlockType == BlockType.CubeAll)
                    _color2Block[color] = blockId;
                else
                    _color2Block.TryAdd(color, blockId);

                _block2Color.Add(blockId, color);
            }

            _color2Block[default] = string.Empty;
            _block2Color[string.Empty] = default;
        }

        private readonly Dictionary<Rgba32, string> _color2Block;
        private readonly Dictionary<string, Rgba32> _block2Color;

        public string this[Rgba32 color] => _color2Block[color];

        public Rgba32 this[string blockId] => _block2Color[blockId];

        public int ColorCount => _color2Block.Count;

        public int BlockCount => _block2Color.Count;

        public IEnumerable<Rgba32> Colors => _color2Block.Keys;

        public IEnumerable<string> Blocks => _block2Color.Keys;

        public Facing Facing { get; }

        public bool ContainsColor(Rgba32 color)
        {
            return _color2Block.ContainsKey(color);
        }

        public bool ContainsBlock(string blockId)
        {
            return _block2Color.ContainsKey(blockId);
        }

        public bool TryGetColor(string blockId, out Rgba32 color)
        {
            return _block2Color.TryGetValue(blockId, out color);
        }

        public bool TryGetBlock(Rgba32 color, [MaybeNullWhen(false)] out string blockId)
        {
            return _color2Block.TryGetValue(color, out blockId);
        }
    }
}
