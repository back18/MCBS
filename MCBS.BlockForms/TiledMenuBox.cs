using MCBS.BlockForms.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.BlockForms
{
    public class TiledMenuBox<T> : MenuBox<T> where T : Control
    {
        public TiledMenuBox()
        {
            _MinWidth = 1;
            _widths = new();
        }

        private readonly List<(T control, int width)> _widths;

        public int MinWidth
        {
            get => _MinWidth;
            set
            {
                if (value < 1)
                    value = 1;

                if (_MinWidth != value)
                {
                    _MinWidth = value;
                    RequestRedraw();
                }
            }
        }
        private int _MinWidth;

        public override void AddedChildControlAndLayout(T control)
        {
            ChildControls.Add(control);
            if (PreviousChildControl is not null)
                control.LayoutRight(this, PreviousChildControl, Spacing);
            else
                control.LayoutRight(this, Spacing, Spacing);

            _items.Add(control);
            _widths.Add((control, control.Width));

            if (control.RightToBorder < 0)
            {
                ActiveLayoutAll();
            }
        }

        public override void RemoveChildControlAndLayout(T control)
        {
            ChildControls.Remove(control);

            _items.Remove(control);
            foreach (var width in _widths)
            {
                if (control == width.control)
                {
                    _widths.Remove(width);
                    break;
                }
            }
            ActiveLayoutAll();
        }

        public override void ActiveLayoutAll()
        {
            int totalWidth = 0;
            foreach (var width in _widths)
                totalWidth += width.width;

            if (totalWidth > ClientSize.Width)
            {
                int width = ClientSize.Width / _items.Count;
                if (width < MinWidth)
                {
                    width = MinWidth;
                    PageSize = new(Math.Max(MinWidth * _items.Count, ClientSize.Width), ClientSize.Height);
                }
                foreach (var item in _items)
                    item.Width = width;
            }
            else
            {
                foreach (var width in _widths)
                    width.control.Width = width.width;
                PageSize = ClientSize;
            }

            T? previous = null;
            foreach (var item in _items)
            {
                if (previous is not null)
                    item.LayoutRight(this, previous, Spacing);
                else
                    item.LayoutRight(this, Spacing, Spacing);
                previous = item;
            }
        }
    }
}
