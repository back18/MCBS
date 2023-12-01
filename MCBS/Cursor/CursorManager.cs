using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Cursor
{
    public class CursorManager : IReadOnlyDictionary<string, CursorContext>
    {
        public CursorManager()
        {
            _items = new();
        }

        private readonly Dictionary<string, CursorContext> _items;

        public CursorContext this[string key] => _items[key];

        public IEnumerable<string> Keys => _items.Keys;

        public IEnumerable<CursorContext> Values => _items.Values;

        public int Count => _items.Count;

        public CursorContext GetOrCreate(string key)
        {
            ArgumentException.ThrowIfNullOrEmpty(key, nameof(key));

            lock (_items)
            {
                if (_items.TryGetValue(key, out var context))
                    return context;
                context = new(key);
                _items.Add(key, context);
                return context;
            }
        }

        public bool ContainsKey(string key)
        {
            return _items.ContainsKey(key);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out CursorContext value)
        {
            return _items.TryGetValue(key, out value);
        }

        public IEnumerator<KeyValuePair<string, CursorContext>> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_items).GetEnumerator();
        }
    }
}
