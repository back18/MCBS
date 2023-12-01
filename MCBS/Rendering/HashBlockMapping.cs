using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Rendering
{
    public class HashBlockMapping : IBlockMapping<int>
    {
        public HashBlockMapping()
        {
            _items = new();
        }

        private readonly Dictionary<int, string> _items;

        public string this[int key] => _items[key];

        public IEnumerable<int> Keys => _items.Keys;

        public IEnumerable<string> Values => _items.Values;

        public int Count => _items.Count;

        public int RegistrationHash(string value)
        {
            ArgumentNullException.ThrowIfNull(value, nameof(value));

            int hash = value.GetHashCode();
            if (!_items.ContainsKey(hash))
            {
                lock (_items)
                    _items.TryAdd(hash, value);
            }
            return hash;
        }

        public bool ContainsKey(int key)
        {
            return _items.ContainsKey(key);
        }

        public bool TryGetValue(int key, [MaybeNullWhen(false)] out string value)
        {
            return _items.TryGetValue(key, out value);
        }

        public IEnumerator<KeyValuePair<int, string>> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_items).GetEnumerator();
        }
    }
}
