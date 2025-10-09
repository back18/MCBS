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
    public class ListMenuBox<T> : MenuBox<T> where T : Control
    {
        public ListMenuBox()
        {
            _MinHeight = 1;
            _initialHeights = [];
        }

        private readonly List<(T control, int height)> _initialHeights;

        public int MinHeight
        {
            get => _MinHeight;
            set
            {
                if (value < 1)
                    value = 1;

                if (_MinHeight != value)
                {
                    _MinHeight = value;
                    RequestRedraw();
                }
            }
        }
        private int _MinHeight;

        public override void AddedChildControlAndLayout(T control)
        {
            ChildControls.Add(control);
            if (PreviousChildControl is not null)
                control.LayoutDown(this, PreviousChildControl, Spacing);
            else
                control.LayoutDown(this, Spacing, Spacing);

            _items.Add(control);
            _initialHeights.Add((control, control.Height));

            if (control.RightToBorder < 0)
                ActiveLayoutAll();
        }

        public override void RemoveChildControlAndLayout(T control)
        {
            ChildControls.Remove(control);
            _items.Remove(control);

            foreach (var initialHeight in _initialHeights)
            {
                if (control == initialHeight.control)
                {
                    control.Height = initialHeight.height;
                    _initialHeights.Remove(initialHeight);
                    break;
                }
            }

            ActiveLayoutAll();
        }

        protected override void OnResize(Control sender, ValueChangedEventArgs<Size> e)
        {
            if (e.NewValue.Height != e.OldValue.Height)
                ActiveLayoutAll();

            base.OnResize(sender, e);
        }

        public override void ActiveLayoutAll()
        {
            int totalHeight = _initialHeights.Sum(w => w.height);
            if (totalHeight > ClientSize.Height)
            {
                int height = ClientSize.Height / _items.Count;
                if (height < MinHeight)
                    height = MinHeight;

                foreach (var item in _items)
                    item.Height = height;

                PageSize = new(ClientSize.Width, Math.Max(MinHeight * _items.Count, ClientSize.Height));
            }
            else
            {
                foreach (var (control, height) in _initialHeights)
                    control.Height = height;
                PageSize = ClientSize;
            }

            T? previous = null;
            foreach (var item in _items)
            {
                if (previous is not null)
                    item.LayoutDown(this, previous, Spacing);
                else
                    item.LayoutDown(this, Spacing, Spacing);
                previous = item;
            }
        }
    }
}
