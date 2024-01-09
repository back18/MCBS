using System;
using System.Collections.Concurrent;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Processes
{
    public partial class ProcessManager
    {
        public class ProcessCollection : IReadOnlyDictionary<Guid, ProcessContext>
        {
            public ProcessCollection(ProcessManager owner)
            {
                ArgumentNullException.ThrowIfNull(owner, nameof(owner));

                _owner = owner;
                _items = new();
            }

            private readonly ProcessManager _owner;

            private readonly ConcurrentDictionary<Guid, ProcessContext> _items;

            public ProcessContext this[Guid key] => _items[key];

            public IEnumerable<Guid> Keys => _items.Keys;

            public IEnumerable<ProcessContext> Values => _items.Values;

            public int Count => _items.Count;

            internal bool TryAdd(Guid key, ProcessContext value)
            {
                if (_items.TryAdd(key, value))
                {
                    _owner.AddedProcess.Invoke(_owner, new(value));
                    return true;
                }

                return false;
            }

            internal bool TryRemove(Guid key, [MaybeNullWhen(false)] out ProcessContext value)
            {
                if (_items.TryRemove(key, out value))
                {
                    _owner.RemovedProcess.Invoke(_owner, new(value));
                    return true;
                }

                return false;
            }

            public bool ContainsKey(Guid key)
            {
                return _items.ContainsKey(key);
            }

            public IEnumerator<KeyValuePair<Guid, ProcessContext>> GetEnumerator()
            {
                return _items.GetEnumerator();
            }

            public bool TryGetValue(Guid key, [MaybeNullWhen(false)] out ProcessContext value)
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
