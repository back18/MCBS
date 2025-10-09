using MCBS.BlockForms.Utility;
using MCBS.Events;
using MCBS.UI;
using MCBS.UI.Extensions;
using QuanLib.Core;
using QuanLib.Core.Events;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.BlockForms
{
    public abstract partial class ContainerControl : AbstractControlContainer<Control>
    {
        protected ContainerControl()
        {
            AddedChildControl += (sender, e) => { };
            RemovedChildControl += (sender, e) => { };
            LayoutAll += OnLayoutAll;
        }

        public abstract ControlCollection<T>? AsControlCollection<T>() where T : Control;

        public override event EventHandler<AbstractControlContainer<Control>, EventArgs<Control>> AddedChildControl;

        public override event EventHandler<AbstractControlContainer<Control>, EventArgs<Control>> RemovedChildControl;

        public event EventHandler<AbstractControlContainer<Control>, ValueChangedEventArgs<Size>> LayoutAll;

        protected override void OnResize(Control sender, ValueChangedEventArgs<Size> e)
        {
            base.OnResize(sender, e);

            LayoutAll.Invoke(this, e);
        }

        public virtual void ActiveLayoutAll()
        {

        }

        protected virtual void OnLayoutAll(AbstractControlContainer<Control> sender, ValueChangedEventArgs<Size> e)
        {
            foreach (var control in GetChildControls())
            {
                if (control.LayoutMode == LayoutMode.Auto)
                    control.HandleLayout(e);
            }
        }

        public override bool HandleCursorMove(CursorEventArgs e)
        {
            UpdateHoverState(e);

            var controls = GetChildControls().ToArray();
            Control? handled = null;

            foreach (var control in controls.Reverse())
            {
                if (control.Visible && control.FirstHandleCursorMove && control.HandleCursorMove(e.Clone(control.ParentPos2ChildPos)))
                {
                    handled = control;
                    goto end;
                }
            }

            foreach (var control in controls)
            {
                if (!control.FirstHandleCursorMove)
                    control.HandleCursorMove(e.Clone(control.ParentPos2ChildPos));
            }

            end:

            foreach (var control in controls)
            {
                if (control == handled)
                    continue;

                if (control.ControlState.HasFlag(ControlState.Hover))
                    control.HandleCursorMove(e.Clone(control.ParentPos2ChildPos));
            }

            return TryInvokeCursorMove(e);
        }

        public override bool HandleRightClick(CursorEventArgs e)
        {
            Control? control = GetChildControls().HoverControlOf(e.CursorContext);
            if (control is not null && control.HandleRightClick(e.Clone(control.ParentPos2ChildPos)) && control.FirstHandleRightClick)
                return true;

            return TryInvokeRightClick(e);
        }

        public override bool HandleLeftClick(CursorEventArgs e)
        {
            Control? control = GetChildControls().HoverControlOf(e.CursorContext);
            if (control is not null && control.HandleLeftClick(e.Clone(control.ParentPos2ChildPos)) && control.FirstHandleLeftClick)
                return true;

            return TryInvokeLeftClick(e);
        }

        public override bool HandleTextEditorUpdate(CursorEventArgs e)
        {
            Control? control = GetChildControls().HoverControlOf(e.CursorContext);
            if (control is not null && control.HandleTextEditorUpdate(e.Clone(control.ParentPos2ChildPos)) && control.FirstHandleTextEditorUpdate)
                return true;

            return TryInvokeTextEditorUpdate(e);
        }

        public override bool HandleCursorSlotChanged(CursorEventArgs e)
        {
            Control? control = GetChildControls().HoverControlOf(e.CursorContext);
            if (control is not null && control.HandleCursorSlotChanged(e.Clone(control.ParentPos2ChildPos)) && control.FirstHandleCursorSlotChanged)
                return true;

            return TryInvokeCursorSlotChanged(e);
        }

        public override bool HandleCursorItemChanged(CursorEventArgs e)
        {
            Control? control = GetChildControls().HoverControlOf(e.CursorContext);
            if (control is not null && control.HandleCursorItemChanged(e.Clone(control.ParentPos2ChildPos)) && control.FirstHandleCursorItemChanged)
                return true;

            return TryInvokeCursorItemChanged(e);
        }

        protected override void DisposeUnmanaged()
        {
            base.DisposeUnmanaged();

            foreach (Control control in GetChildControls())
                control.Dispose();
        }
    }
}
