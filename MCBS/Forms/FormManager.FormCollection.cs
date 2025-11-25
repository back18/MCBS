using MCBS.ObjectModel;
using QuanLib.Core.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Forms
{
    public partial class FormManager
    {
        public class FormCollection : IEnumerable<FormContext>
        {
            public FormCollection(FormManager owner)
            {
                ArgumentNullException.ThrowIfNull(owner, nameof(owner));

                _owner = owner;
                _items = [];

                _items.Added += Items_Added;
                _items.Removed += Items_Removed;
            }

            private readonly FormManager _owner;

            private readonly GuidDictionary<FormContext> _items;

            public int Count => _items.Count;

            public FormContext GetForm(Guid guid)
            {
                return _items[guid];
            }

            public FormContext GetForm(string shortId)
            {
                return _items[shortId];
            }

            public FormContext[] GetForms()
            {
                return _items.Values.ToArray();
            }

            internal void AddForm(FormContext FormContext)
            {
                _items.Add(FormContext);
            }

            internal bool RemoveForm(Guid guid)
            {
                return _items.Remove(guid);
            }

            internal bool RemoveForm(string shortId)
            {
                return _items.Remove(shortId);
            }

            internal bool RemoveForm(FormContext FormContext)
            {
                return _items.Remove(FormContext);
            }

            internal void ClearAllForm()
            {
                _items.Clear();
            }

            public bool ContainsForm(Guid guid)
            {
                return _items.ContainsKey(guid);
            }

            public bool ContainsForm(string shortId)
            {
                return _items.ContainsKey(shortId);
            }

            public bool ContainsForm(FormContext FormContext)
            {
                ArgumentNullException.ThrowIfNull(FormContext, nameof(FormContext));

                return _items.ContainsKey(FormContext.Guid);
            }

            public bool TryGetForm(Guid guid, [MaybeNullWhen(false)] out FormContext result)
            {
                return _items.TryGetValue(guid, out result);
            }

            public bool TryGetForm(string shortId, [MaybeNullWhen(false)] out FormContext result)
            {
                return _items.TryGetValue(shortId, out result);
            }

            public Guid PreGenerateGuid()
            {
                return _items.PreGenerateGuid();
            }

            private void Items_Added(GuidDictionary<FormContext> sender, EventArgs<FormContext> e)
            {
                _owner.AddedForm.Invoke(_owner, e);
            }

            private void Items_Removed(GuidDictionary<FormContext> sender, EventArgs<FormContext> e)
            {
                _owner.RemovedForm.Invoke(_owner, e);
            }

            public IEnumerator<FormContext> GetEnumerator()
            {
                return _items.Values.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}
