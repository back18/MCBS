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
        public Rgba32BlockMapping(BlockTextureManager blockTextureManager, Facing facing)
        {
            if (blockTextureManager is null)
                throw new ArgumentNullException(nameof(blockTextureManager));

            Facing = facing;

            _items = new();
            foreach (var texture in blockTextureManager.Values)
            {
                if (texture.BlockType == BlockType.CubeAll)
                    _items[texture.Textures[facing].AverageColor] = texture.BlockID;
                else
                    _items.TryAdd(texture.Textures[facing].AverageColor, texture.BlockID);
            }
        }

        private readonly Dictionary<Rgba32, string> _items;

        public string this[Rgba32 key] => _items[key];

        public IEnumerable<Rgba32> Keys => _items.Keys;

        public IEnumerable<string> Values => _items.Values;

        public int Count => _items.Count;

        public Facing Facing { get; }

        public ColorMatcher<TPixel> CreateColorMatcher<TPixel>() where TPixel : unmanaged, IPixel<TPixel>
        {
            return new(Keys);
        }

        public bool ContainsKey(Rgba32 key)
        {
            return _items.ContainsKey(key);
        }

        public bool TryGetValue(Rgba32 key, [MaybeNullWhen(false)] out string value)
        {
            return _items.TryGetValue(key, out value);
        }

        public IEnumerator<KeyValuePair<Rgba32, string>> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_items).GetEnumerator();
        }
    }
}
