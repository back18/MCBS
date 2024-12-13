using QuanLib.Minecraft.ResourcePack.Block;
using QuanLib.Minecraft;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuanLib.Core;

namespace MCBS.Drawing
{
    public class HashBlockMapping : IBlockMapping<int>
    {
        public HashBlockMapping(BlockTextureManager blockTextureManager, IEnumerable<BlockState> blacklist)
        {
            ArgumentNullException.ThrowIfNull(blockTextureManager, nameof(blockTextureManager));
            CollectionValidator.ValidateNull(blacklist, nameof(blacklist));

            _keys = [];
            _values = [];

            foreach (string blockId in blockTextureManager.Keys)
            {
                if (!BlockState.TryParse(blockId, out var blockState))
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

                int hash = blockId.GetHashCode();
                _keys.Add(blockId, hash);
                _values.Add(hash, blockId);
            }
        }

        private readonly object _lock = new();

        private readonly Dictionary<string, int> _keys;

        private readonly Dictionary<int, string> _values;

        public string this[int key] => _values[key];

        public IEnumerable<int> Keys => _values.Keys;

        public IEnumerable<string> Values => _values.Values;

        public int Count => _values.Count;

        public bool ContainsKey(int key)
        {
            return _values.ContainsKey(key);
        }

        public bool TryGetValue(int key, [MaybeNullWhen(false)] out string value)
        {
            return _values.TryGetValue(key, out value);
        }

        public bool TryGetKey(string value, out int key)
        {
            return _keys.TryGetValue(value, out key);
        }

        public IEnumerator<KeyValuePair<int, string>> GetEnumerator()
        {
            return _values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_values).GetEnumerator();
        }
    }
}
