using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Drawing
{
    public class HashBlockMapping : IBlockMapping<int>
    {
        public HashBlockMapping(string[] blockIds)
        {
            ArgumentNullException.ThrowIfNull(blockIds, nameof(blockIds));

            _block2Color = [];
            _color2Block = [];

            foreach (string blockId in blockIds)
                TryAdd(blockId);

            TryAdd("minecraft:air");
            TryAdd(string.Empty);

            bool TryAdd(string blockId)
            {
                if (_block2Color.ContainsKey(blockId))
                    return false;

                int hash = blockId.GetHashCode();
                _color2Block.Add(hash, blockId);
                _block2Color.Add(blockId, hash);
                return true;
            }
        }

        private readonly Dictionary<int, string> _color2Block;
        private readonly Dictionary<string, int> _block2Color;

        public string this[int color] => _color2Block[color];

        public int this[string blockId] => _block2Color[blockId];

        public IEnumerable<int> Colors => _color2Block.Keys;

        public IEnumerable<string> Blocks => _block2Color.Keys;

        public int ColorCount => _color2Block.Count;

        public int BlockCount => _block2Color.Count;

        public bool ContainsColor(int color)
        {
            return _color2Block.ContainsKey(color);
        }

        public bool ContainsBlock(string blockId)
        {
            return _block2Color.ContainsKey(blockId);
        }

        public bool TryGetColor(string blockId, out int color)
        {
            return _block2Color.TryGetValue(blockId, out color);
        }

        public bool TryGetBlock(int color, [MaybeNullWhen(false)] out string value)
        {
            return _color2Block.TryGetValue(color, out value);
        }
    }
}
