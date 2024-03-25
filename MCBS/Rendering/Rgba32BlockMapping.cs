using QuanLib.Game;
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
        public Rgba32BlockMapping(BlockTextureManager blockTextureManager, Facing facing)
        {
            ArgumentNullException.ThrowIfNull(blockTextureManager, nameof(blockTextureManager));

            Facing = facing;

            _items1 = [];
            _items2 = [];
            foreach (var texture in blockTextureManager.Values)
            {
                if (texture.BlockType == BlockType.CubeAll)
                    _items1[texture.Textures[facing].AverageColor] = texture.BlockID;
                else
                    _items1.TryAdd(texture.Textures[facing].AverageColor, texture.BlockID);

                _items2.Add(texture.BlockID, texture.Textures[facing].AverageColor);
            }

            _items1[default] = string.Empty;
            _items2[string.Empty] = default;
        }

        private readonly Dictionary<Rgba32, string> _items1;
        private readonly Dictionary<string, Rgba32> _items2;

        public string this[Rgba32 key] => _items1[key];

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
