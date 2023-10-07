using MCBS.Events;
using MCBS.UI;
using QuanLib.Core;
using SixLabors.ImageSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.BlockForms
{
    public abstract class ContainerControl<TControl> : ContainerControl where TControl : Control
    {
        protected ContainerControl()
        {
            ChildControls = new(this);
        }

        public ControlCollection<TControl> ChildControls { get; }

        public override IReadOnlyControlCollection<Control> GetChildControls() => ChildControls;

        public override ControlCollection<T>? AsControlCollection<T>()
        {
            if (ChildControls is ControlCollection<T> result)
                return result;

            return null;
        }

        public override void ClearAllLayoutSyncer()
        {
            foreach (Control control in ChildControls)
            {
                control.ClearAllLayoutSyncer();
            }

            base.ClearAllLayoutSyncer();
        }
    }

    public abstract class ContainerControl : AbstractContainer<Control>
    {
        protected ContainerControl()
        {
            AddedChildControl += (sender, e) => { };
            RemovedChildControl += (sender, e) => { };
            LayoutAll += OnLayoutAll;
        }

        public abstract ControlCollection<T>? AsControlCollection<T>() where T : Control;

        public override event EventHandler<AbstractContainer<Control>, ControlEventArgs<Control>> AddedChildControl;

        public override event EventHandler<AbstractContainer<Control>, ControlEventArgs<Control>> RemovedChildControl;

        public event EventHandler<AbstractContainer<Control>, SizeChangedEventArgs> LayoutAll;

        protected override void OnResize(Control sender, SizeChangedEventArgs e)
        {
            base.OnResize(sender, e);

            LayoutAll.Invoke(this, e);
        }

        public virtual void ActiveLayoutAll()
        {

        }

        protected virtual void OnLayoutAll(AbstractContainer<Control> sender, SizeChangedEventArgs e)
        {
            foreach (var control in GetChildControls())
            {
                if (control.LayoutMode == LayoutMode.Auto)
                    control.HandleLayout(e);
            }
        }

        public override bool HandleRightClick(CursorEventArgs e)
        {
            Control? control = GetChildControls().FirstHover;
            if (control is not null && control.HandleRightClick(e.Clone(control.ParentPos2ChildPos)) && control.FirstHandleRightClick)
                return true;

            return TryHandleRightClick(e);
        }

        public override bool HandleLeftClick(CursorEventArgs e)
        {
            Control? control = GetChildControls().FirstHover;
            if (control is not null && control.HandleLeftClick(e.Clone(control.ParentPos2ChildPos)) && control.FirstHandleLeftClick)
                return true;

            return TryHandleLeftClick(e);
        }

        public override bool HandleTextEditorUpdate(CursorEventArgs e)
        {
            Control? control = GetChildControls().FirstHover;
            if (control is not null && control.HandleTextEditorUpdate(e.Clone(control.ParentPos2ChildPos)) && control.FirstHandleTextEditorUpdate)
                return true;

            return TryHandleTextEditorUpdate(e);
        }

        public override bool HandleCursorSlotChanged(CursorEventArgs e)
        {
            Control? control = GetChildControls().FirstHover;
            if (control is not null && control.HandleCursorSlotChanged(e.Clone(control.ParentPos2ChildPos)) && control.FirstHandleCursorSlotChanged)
                return true;

            return TryHandleCursorSlotChanged(e);
        }

        public override bool HandleCursorItemChanged(CursorEventArgs e)
        {
            Control? control = GetChildControls().FirstHover;
            if (control is not null && control.HandleCursorItemChanged(e.Clone(control.ParentPos2ChildPos)) && control.FirstHandleCursorItemChanged)
                return true;

            return TryHandleCursorItemChanged(e);
        }

        public override void UpdateHoverState(CursorEventArgs e)
        {
            foreach (var control in GetChildControls().ToArray())
            {
                control.UpdateHoverState(e.Clone(control.ParentPos2ChildPos));
            }

            base.UpdateHoverState(e);
        }

        public class ControlCollection<T> : AbstractControlCollection<T> where T : Control
        {
            public ControlCollection(ContainerControl owner)
            {
                _owner = owner ?? throw new ArgumentNullException(nameof(owner));
            }

            private readonly ContainerControl _owner;

            public override void Add(T item)
            {
                if (item is null)
                    throw new ArgumentNullException(nameof(item));

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

                ((IControl)item).SetGenericContainerControl(_owner);
                RecentlyAddedControl = item;
                _owner.AddedChildControl.Invoke(_owner, new(item));
                _owner.RequestUpdateFrame();
            }

            public override bool Remove(T item)
            {
                if (item is null)
                    throw new ArgumentNullException(nameof(item));

                if (!_items.Remove(item))
                    return false;

                ((IControl)item).SetGenericContainerControl(null);
                RecentlyRemovedControl = item;
                _owner.RemovedChildControl.Invoke(_owner, new(item));
                _owner.RequestUpdateFrame();
                return true;
            }

            public override void Clear()
            {
                foreach (var item in _items.ToArray())
                    if (!item.KeepWhenClear)
                        Remove(item);
                RecentlyAddedControl = null;
            }

            public void ClearSelecteds()
            {
                foreach (var control in _items.ToArray())
                    control.IsSelected = false;
            }

            public void ClearSyncers()
            {
                foreach (var control in _items)
                    control.LayoutSyncer = null;
            }
        }
    }
}
