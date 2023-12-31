﻿using MCBS.Cursor;
using MCBS.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.BlockForms.Utility
{
    public class AbstractControlCollection<T> : IList<T>, IReadOnlyControlCollection<T> where T : class, IControl
    {
        public AbstractControlCollection()
        {
            _items = new();
        }

        protected readonly List<T> _items;

        public int Count => _items.Count;

        public bool IsReadOnly => false;

        public bool HaveHover => FirstHover is not null;

        public bool HaveSelected => FirstSelected is not null;

        public T? FirstHover
        {
            get
            {
                for (int i = _items.Count - 1; i >= 0; i--)
                {
                    if (_items[i].IsHover)
                        return _items[i];
                }
                return null;
            }
        }

        public T? FirstSelected
        {
            get
            {
                for (int i = _items.Count - 1; i >= 0; i--)
                {
                    if (_items[i].IsSelected)
                        return _items[i];
                }
                return null;
            }
        }

        public T? RecentlyAddedControl { get; protected set; }

        public T? RecentlyRemovedControl { get; protected set; }

        public T this[int index] => _items[index];

        T IList<T>.this[int index] { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        public virtual void Add(T item)
        {
            ArgumentNullException.ThrowIfNull(item, nameof(item));

            _items.Add(item);
        }

        public bool TryAdd(T item)
        {
            if (_items.Contains(item))
                return false;

            RecentlyAddedControl = item;
            Add(item);
            return true;
        }

        public virtual bool Remove(T item)
        {
            ArgumentNullException.ThrowIfNull(item, nameof(item));

            if (!_items.Remove(item))
                return false;

            RecentlyRemovedControl = item;
            return true;
        }

        public void RemoveAt(int index)
        {
            Remove(_items[index]);
        }

        public virtual void Clear()
        {
            foreach (var item in _items.ToArray())
                Remove(item);
            RecentlyAddedControl = null;
        }

        public bool Contains(T item)
        {
            return _items.Contains(item);
        }

        public int IndexOf(T item)
        {
            return _items.IndexOf(item);
        }

        public T[] GetHovers()
        {
            List<T> result = new();
            foreach (var item in _items)
            {
                if (item.IsHover)
                    result.Add(item);
            }
            return result.ToArray();
        }

        public T[] GetSelecteds()
        {
            List<T> result = new();
            foreach (var item in _items)
            {
                if (item.IsSelected)
                    result.Add(item);
            }
            return result.ToArray();
        }

        public virtual T[] ToArray()
        {
            return _items.ToArray();
        }

        public T? HoverControlOf(CursorContext cursorContext)
        {
            for (int i = _items.Count - 1; i >= 0; i--)
            {
                if (_items[i].IsHover && Array.IndexOf(_items[i].GetHoverCursors(), cursorContext) != -1)
                    return _items[i];
            }
            return null;
        }

        public virtual void Sort()
        {
            _items.Sort();
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _items.CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_items).GetEnumerator();
        }

        void IList<T>.Insert(int index, T item)
        {
            throw new NotSupportedException();
        }
    }
}
