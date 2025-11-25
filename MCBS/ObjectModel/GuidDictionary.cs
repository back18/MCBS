using QuanLib.Core;
using QuanLib.Core.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.ObjectModel
{
    public class GuidDictionary<TValue> : IReadOnlyDictionary<Guid, TValue>, IReadOnlyDictionary<string, TValue>
    {
        public GuidDictionary()
        {
            _items = [];
            _pregen = [];

            Added += OnAdded;
            Removed += OnRemoved;
        }

        private readonly Dictionary<Guid, TValue> _items;

        private readonly HashSet<Guid> _pregen;

        public TValue this[Guid key] => _items[key];

        public IEnumerable<Guid> Keys => _items.Keys;

        public IEnumerable<TValue> Values => _items.Values;

        public int Count => _items.Count;

        public TValue this[string key]
        {
            get
            {
                if (TryGetValue(key, out var value))
                    return value;
                else
                    throw new KeyNotFoundException($"The given key '{key}' was not present in the dictionary.");
            }
        }

        IEnumerable<string> IReadOnlyDictionary<string, TValue>.Keys
        {
            get
            {
                return _items.Keys.Select(s => s.GetShortId());
            }
        }

        public event EventHandler<GuidDictionary<TValue>, EventArgs<TValue>> Added;

        public event EventHandler<GuidDictionary<TValue>, EventArgs<TValue>> Removed;

        protected virtual void OnAdded(GuidDictionary<TValue> sender, EventArgs<TValue> e) { }

        protected virtual void OnRemoved(GuidDictionary<TValue> sender, EventArgs<TValue> e) { }

        public void Add(Guid key, TValue value)
        {
            ArgumentNullException.ThrowIfNull(value, nameof(value));

            _pregen.Remove(key);
            _items.Add(key, value);
            Added.Invoke(this, new(value));
        }

        public void Add<T>(T value) where T : TValue, IUnique
        {
            ArgumentNullException.ThrowIfNull(value, nameof(value));

            Add(value.Guid, value);
        }

        public bool Remove(Guid key)
        {
            if (_items.Remove(key, out var value))
            {
                Removed.Invoke(this, new(value));
                return true;
            }

            return false;
        }

        public bool Remove(string key)
        {
            ArgumentNullException.ThrowIfNull(key, nameof(key));

            if (key.Length != 4)
                return false;

            foreach (Guid guid in _items.Keys)
            {
                if (guid.GetShortId().Equals(key, StringComparison.OrdinalIgnoreCase))
                    return Remove(guid);
            }

            return false;
        }

        public bool Remove<T>(T value) where T : TValue, IUnique
        {
            ArgumentNullException.ThrowIfNull(value, nameof(value));

            return Remove(value.Guid);
        }

        public void Clear()
        {
            TValue[] values = Values.ToArray();
            _items.Clear();

            List<Exception>? exceptions = null;
            foreach (TValue value in values)
            {
                try
                {
                    Removed.Invoke(this, new(value));
                }
                catch (Exception ex)
                {
                    exceptions ??= [];
                    exceptions?.Add(ex);
                }
            }

            if (exceptions is not null && exceptions.Count > 0)
                throw new AggregateException(exceptions);
        }

        public bool ContainsKey(Guid key)
        {
            return _items.ContainsKey(key);
        }

        public bool TryGetValue(Guid key, [MaybeNullWhen(false)] out TValue value)
        {
            return _items.TryGetValue(key, out value);
        }

        public bool ContainsKey(string key)
        {
            ArgumentNullException.ThrowIfNull(key, nameof(key));

            if (key.Length != 4)
                return false;

            foreach (Guid guid in _items.Keys)
            {
                if (guid.GetShortId().Equals(key, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out TValue value)
        {
            ArgumentNullException.ThrowIfNull(key, nameof(key));

            if (key.Length != 4)
            {
                value = default;
                return false;
            }

            foreach (Guid guid in _items.Keys)
            {
                if (guid.GetShortId().Equals(key, StringComparison.OrdinalIgnoreCase))
                    return TryGetValue(guid, out value);
            }

            value = default;
            return false;
        }

        public Guid PreGenerateGuid()
        {
            List<Guid> guids = _items.Keys.ToList();
            guids.AddRange(_pregen);
            Guid guid = GuidHelper.GenerateShortId(guids, -1);
            _pregen.Add(guid);
            return guid;
        }

        public IEnumerator<KeyValuePair<Guid, TValue>> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator<KeyValuePair<string, TValue>> IEnumerable<KeyValuePair<string, TValue>>.GetEnumerator()
        {
            foreach (var item in _items)
                yield return new KeyValuePair<string, TValue>(item.Key.GetShortId(), item.Value);
        }
    }
}
