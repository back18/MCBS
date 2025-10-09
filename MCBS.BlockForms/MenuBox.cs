using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.BlockForms
{
    public abstract class MenuBox<T> : ScrollablePanel where T : Control
    {
        protected MenuBox()
        {
            Spacing = 1;
            _items = [];
        }

        protected readonly List<T> _items;

        protected T? PreviousChildControl
        {
            get
            {
                if (_items.Count == 0)
                    return null;
                else
                    return _items[^1];
            }
        }

        public int Spacing
        {
            get => _Spacing;
            set
            {
                if (value < 0)
                    value = 0;
                if (_Spacing != value)
                {
                    _Spacing = value;
                    RequestRedraw();
                }
            }
        }
        private int _Spacing;

        public abstract void AddedChildControlAndLayout(T control);

        public abstract void RemoveChildControlAndLayout(T control);
    }
}
