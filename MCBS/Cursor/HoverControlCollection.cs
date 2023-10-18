using MCBS.UI;
using SixLabors.ImageSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Cursor
{
    public class HoverControlCollection : IReadOnlyDictionary<IControl, HoverControl>
    {
        public HoverControlCollection()
        {
            _items = new();
        }

        private readonly Dictionary<IControl, HoverControl> _items;

        public HoverControl this[IControl key] => _items[key];

        public IEnumerable<IControl> Keys => _items.Keys;

        public IEnumerable<HoverControl> Values => _items.Values;

        public int Count => _items.Count;

        public bool TryAdd(IControl control, Point offsetPosition, [MaybeNullWhen(false)] out HoverControl result)
        {
            if (control is null)
                throw new ArgumentNullException(nameof(control));

            if (_items.ContainsKey(control))
            {
                result = null;
                return false;
            }

            result = new(control, offsetPosition);
            _items.Add(control, result);
            return true;
        }

        public bool TryAdd(IControl control, [MaybeNullWhen(false)] out HoverControl result)
        {
            if (control is null)
                throw new ArgumentNullException(nameof(control));

            if (_items.ContainsKey(control))
            {
                result = null;
                return false;
            }

            result = new(control);
            _items.Add(control, result);
            return true;
        }

        public bool TryRemove(IControl control, [MaybeNullWhen(false)] out HoverControl result)
        {
            if (control is null)
                throw new ArgumentNullException(nameof(control));

            if (_items.TryGetValue(control, out result))
            {
                _items.Remove(control);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool ContainsKey(IControl key)
        {
            if (key is null)
                throw new ArgumentNullException(nameof(key));

            return _items.ContainsKey(key);
        }

        public bool TryGetValue(IControl key, [MaybeNullWhen(false)] out HoverControl value)
        {
            if (key is null)
                throw new ArgumentNullException(nameof(key));

            return _items.TryGetValue(key, out value);
        }

        public IEnumerator<KeyValuePair<IControl, HoverControl>> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_items).GetEnumerator();
        }
    }
}
