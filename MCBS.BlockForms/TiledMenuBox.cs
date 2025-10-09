using MCBS.BlockForms.Utility;
using QuanLib.Core.Events;
using SixLabors.ImageSharp;
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
            _initialWidths = [];
        }

        private readonly List<(T control, int width)> _initialWidths;

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
            _initialWidths.Add((control, control.Width));

            if (control.RightToBorder < 0)
                ActiveLayoutAll();
        }

        public override void RemoveChildControlAndLayout(T control)
        {
            ChildControls.Remove(control);
            _items.Remove(control);

            foreach (var initialWidth in _initialWidths)
            {
                if (control == initialWidth.control)
                {
                    control.Width = initialWidth.width;
                    _initialWidths.Remove(initialWidth);
                    break;
                }
            }

            ActiveLayoutAll();
        }

        protected override void OnResize(Control sender, ValueChangedEventArgs<Size> e)
        {
            if (e.NewValue.Width != e.OldValue.Width)
                ActiveLayoutAll();

            base.OnResize(sender, e);
        }

        public override void ActiveLayoutAll()
        {
            int totalWidth = _initialWidths.Sum(w => w.width);
            if (totalWidth > ClientSize.Width)
            {
                int width = ClientSize.Width / _items.Count;
                if (width < MinWidth)
                    width = MinWidth;

                foreach (var item in _items)
                    item.Width = width;

                PageSize = new(Math.Max(MinWidth * _items.Count, ClientSize.Width), ClientSize.Height);
            }
            else
            {
                foreach (var (control, width) in _initialWidths)
                    control.Width = width;
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
