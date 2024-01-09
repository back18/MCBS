using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Forms
{
    public partial class FormManager
    {
        public class FormCollection : IReadOnlyDictionary<Guid, FormContext>
        {
            public FormCollection(FormManager owner)
			{
                ArgumentNullException.ThrowIfNull(owner, nameof(owner));

				_owner = owner;
				_items = new();
            }

            private readonly FormManager _owner;

			private readonly ConcurrentDictionary<Guid, FormContext> _items;

            public FormContext this[Guid key] => _items[key];

            public IEnumerable<Guid> Keys => _items.Keys;

            public IEnumerable<FormContext> Values => _items.Values;

            public int Count => _items.Count;

            internal bool TryAdd(Guid key, FormContext value)
            {
                if (_items.TryAdd(key, value))
                {
                    _owner.AddedForm.Invoke(_owner, new(value));
                    return true;
                }

                return false;
            }

            internal bool TryRemove(Guid key, [MaybeNullWhen(false)] out FormContext value)
            {
                if (_items.TryRemove(key, out value))
                {
                    _owner.RemovedForm.Invoke(_owner, new(value));
                    return true;
                }

                return false;
            }

            public bool ContainsKey(Guid key)
            {
                return _items.ContainsKey(key);
            }

            public IEnumerator<KeyValuePair<Guid, FormContext>> GetEnumerator()
            {
                return _items.GetEnumerator();
            }

            public bool TryGetValue(Guid key, [MaybeNullWhen(false)] out FormContext value)
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
