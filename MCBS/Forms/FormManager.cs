using MCBS.Application;
using MCBS.Events;
using MCBS.UI;
using QuanLib.Core;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Forms
{
    public class FormManager : ITickable
    {
        public FormManager()
        {
            Items = new(this);

            AddedForm += OnAddedForm;
            RemovedForm += OnRemovedForm;
        }

        public FormCollection Items { get; }

        public event EventHandler<FormManager, FormContextEventArgs> AddedForm;

        public event EventHandler<FormManager, FormContextEventArgs> RemovedForm;

        protected virtual void OnAddedForm(FormManager sender, FormContextEventArgs e) { }

        protected virtual void OnRemovedForm(FormManager sender, FormContextEventArgs e) { }

        public void OnTick()
        {
            foreach (var context in Items)
            {
                context.Value.OnTick();
                if (context.Value.FormState == FormState.Closed)
                    Items.Remove(context.Key);
            }
        }

        public class FormCollection : IDictionary<int, FormContext>
        {
            public FormCollection(FormManager owner)
            {
                ArgumentNullException.ThrowIfNull(owner, nameof(owner));

                _owner = owner;
                _items = new();
            }

            private readonly FormManager _owner;

            private readonly ConcurrentDictionary<int, FormContext> _items;

            private int _id;

            public ICollection<int> Keys => _items.Keys;

            public ICollection<FormContext> Values => _items.Values;

            public int Count => _items.Count;

            public bool IsReadOnly => false;

            public FormContext this[int index] => _items[index];

            FormContext IDictionary<int, FormContext>.this[int index] { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

            public FormContext Add(IProgram program, IForm form)
            {
                ArgumentNullException.ThrowIfNull(program, nameof(program));
                ArgumentNullException.ThrowIfNull(form, nameof(form));

                lock (_items)
                {
                    int id = _id;
                    FormContext context = new(program, form);
                    context.ID = id;
                    _items.TryAdd(id, context);
                    _owner.AddedForm.Invoke(_owner, new(context));
                    _id++;
                    return context;
                }
            }

            public bool Remove(int id)
            {
                lock (_items)
                {
                    if (!_items.TryGetValue(id, out var context) || !_items.TryRemove(id, out _))
                        return false;

                    context.ID = -1;
                    _owner.RemovedForm.Invoke(_owner, new(context));
                    return true;
                }
            }

            public void Clear()
            {
                foreach (var id in _items.Keys.ToArray())
                    Remove(id);
            }

            public bool ContainsKey(int id)
            {
                return _items.ContainsKey(id);
            }

            public bool TryGetValue(int id, [MaybeNullWhen(false)] out FormContext context)
            {
                return _items.TryGetValue(id, out context);
            }

            public IEnumerator<KeyValuePair<int, FormContext>> GetEnumerator()
            {
                return _items.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable)_items).GetEnumerator();
            }

            void ICollection<KeyValuePair<int, FormContext>>.Add(KeyValuePair<int, FormContext> item)
            {
                throw new NotSupportedException();
            }

            bool ICollection<KeyValuePair<int, FormContext>>.Remove(KeyValuePair<int, FormContext> item)
            {
                throw new NotSupportedException();
            }

            bool ICollection<KeyValuePair<int, FormContext>>.Contains(KeyValuePair<int, FormContext> item)
            {
                throw new NotSupportedException();
            }

            void ICollection<KeyValuePair<int, FormContext>>.CopyTo(KeyValuePair<int, FormContext>[] array, int arrayIndex)
            {
                throw new NotSupportedException();
            }

            void IDictionary<int, FormContext>.Add(int key, FormContext value)
            {
                throw new NotSupportedException();
            }
        }
    }
}
