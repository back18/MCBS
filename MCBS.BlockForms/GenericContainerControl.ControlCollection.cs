using MCBS.BlockForms.Utility;
using MCBS.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.BlockForms
{
    public abstract partial class GenericContainerControl
    {
        public class ControlCollection<T> : AbstractControlCollection<T> where T : class, IControl
        {
            public ControlCollection(GenericContainerControl owner)
            {
                ArgumentNullException.ThrowIfNull(owner, nameof(owner));

                _owner = owner;
            }

            private readonly GenericContainerControl _owner;

            public override void Add(T item)
            {
                ArgumentNullException.ThrowIfNull(item, nameof(item));

                bool insert = false;
                for (int i = _items.Count - 1; i >= 0; i--)
                {
                    if (item.DisplayPriority >= _items[i].DisplayPriority)
                    {
                        _items.Insert(i + 1, item);
                        insert = true;
                        break;
                    }
                }
                if (!insert)
                    _items.Insert(0, item);

                item.SetGenericContainerControl(_owner);
                RecentlyAddedControl = item;
                _owner.AddedChildControl.Invoke(_owner, new(item));
                _owner.RequestRendering();
            }

            public override bool Remove(T item)
            {
                ArgumentNullException.ThrowIfNull(item, nameof(item));

                if (!_items.Remove(item))
                    return false;

                item.SetGenericContainerControl(null);
                RecentlyRemovedControl = item;
                _owner.RemovedChildControl.Invoke(_owner, new(item));
                _owner.RequestRendering();
                return true;
            }
        }
    }
}
