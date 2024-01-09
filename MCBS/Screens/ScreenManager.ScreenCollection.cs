using System;
using System.Collections.Concurrent;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Screens
{
    public partial class ScreenManager
    {
        public class ScreenCollection : IReadOnlyDictionary<Guid, ScreenContext>
        {
            public ScreenCollection(ScreenManager owner)
            {
                ArgumentNullException.ThrowIfNull(owner, nameof(owner));

                _owner = owner;
                _items = new();
            }

            private readonly ScreenManager _owner;

            private readonly ConcurrentDictionary<Guid, ScreenContext> _items;

            public ScreenContext this[Guid key] => _items[key];

            public IEnumerable<Guid> Keys => _items.Keys;

            public IEnumerable<ScreenContext> Values => _items.Values;

            public int Count => _items.Count;

            internal bool TryAdd(Guid key, ScreenContext value)
            {
                if (_items.TryAdd(key, value))
                {
                    _owner.AddedScreen.Invoke(_owner, new(value));
                    return true;
                }

                return false;
            }

            internal bool TryRemove(Guid key, [MaybeNullWhen(false)] out ScreenContext value)
            {
                if (_items.TryRemove(key, out value))
                {
                    _owner.RemovedScreen.Invoke(_owner, new(value));
                    return true;
                }

                return false;
            }

            public bool ContainsKey(Guid key)
            {
                return _items.ContainsKey(key);
            }

            public IEnumerator<KeyValuePair<Guid, ScreenContext>> GetEnumerator()
            {
                return _items.GetEnumerator();
            }

            public bool TryGetValue(Guid key, [MaybeNullWhen(false)] out ScreenContext value)
            {
                return _items.TryGetValue(key, out value);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable)_items).GetEnumerator();
            }
        }
    }
}
