using QuanLib.Core;
using QuanLib.Game;
using QuanLib.Minecraft;
using QuanLib.Minecraft.ResourcePack.Block;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Rendering
{
    public class Rgba32BlockMapping : IBlockMapping<Rgba32>
    {
        public Rgba32BlockMapping(BlockTextureManager blockTextureManager, Facing facing, IEnumerable<BlockState> blacklist)
        {
            ArgumentNullException.ThrowIfNull(blockTextureManager, nameof(blockTextureManager));
            CollectionValidator.ValidateNull(blacklist, nameof(blacklist));

            Facing = facing;
            _items1 = [];
            _items2 = [];

            foreach (BlockTexture blockTexture in blockTextureManager.Values)
            {
                if (!BlockState.TryParse(blockTexture.BlockId, out var blockState))
                    continue;

                bool isBlacklist = false;
                foreach (BlockState blacklistBlockState in blacklist)
                {
                    if (blacklistBlockState.BlockId != blockState.BlockId)
                        continue;

                    foreach (var item in blacklistBlockState.States)
                    {
                        if (!blockState.States.TryGetValue(item.Key, out var value) || value != item.Value)
                            continue;
                    }

                    isBlacklist = true;
                }

                if (isBlacklist)
                    continue;

                if (blockTexture.BlockType == BlockType.CubeAll)
                    _items1[blockTexture.Textures[facing].AverageColor] = blockTexture.BlockId;
                else
                    _items1.TryAdd(blockTexture.Textures[facing].AverageColor, blockTexture.BlockId);

                _items2.Add(blockTexture.BlockId, blockTexture.Textures[facing].AverageColor);
            }

            _items1[default] = string.Empty;
            _items2[string.Empty] = default;
        }

        private readonly Dictionary<Rgba32, string> _items1;
        private readonly Dictionary<string, Rgba32> _items2;

        public string this[Rgba32 key] => _items1[key];

        public Rgba32 this[string value] => _items2[value];

        public IEnumerable<Rgba32> Keys => _items1.Keys;

        public IEnumerable<string> Values => _items1.Values;

        public int Count => _items1.Count;

        public Facing Facing { get; }

        public ColorMatcher<TPixel> CreateColorMatcher<TPixel>() where TPixel : unmanaged, IPixel<TPixel>
        {
            SR.ColorMappingCaches.TryGetValue(Facing, out var mappingCache);
            return new(Keys.ToHashSet(), mappingCache);
        }

        public bool ContainsKey(Rgba32 key)
        {
            return _items1.ContainsKey(key);
        }

        public bool TryGetValue(Rgba32 key, [MaybeNullWhen(false)] out string value)
        {
            return _items1.TryGetValue(key, out value);
        }

        public bool TryGetKey(string value, out Rgba32 key)
        {
            return _items2.TryGetValue(value, out key);
        }

        public IEnumerator<KeyValuePair<Rgba32, string>> GetEnumerator()
        {
            return _items1.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_items1).GetEnumerator();
        }
    }
}
