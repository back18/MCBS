using MCBS.Cursor;
using MCBS.Events;
using MCBS.Rendering;
using MCBS.UI;
using QuanLib.Core;
using SixLabors.ImageSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.BlockForms
{
    public abstract class AbstractControlContainer<TControl> : Control, IContainerControl where TControl : class, IControl
    {
        protected AbstractControlContainer()
        {
            AddedChildControl += OnAddedChildControl;
            RemovedChildControl += OnRemovedChildControl;
        }

        public Type ChildControlType => typeof(TControl);

        public bool IsChildControlType<T>() => typeof(T) == ChildControlType;

        public abstract event EventHandler<AbstractControlContainer<TControl>, ControlEventArgs<TControl>> AddedChildControl;

        public abstract event EventHandler<AbstractControlContainer<TControl>, ControlEventArgs<TControl>> RemovedChildControl;

        protected virtual void OnAddedChildControl(AbstractControlContainer<TControl> sender, ControlEventArgs<TControl> e)
        {
            IControlInitializeHandling handling = e.Control;
            if (IsInitCompleted && !handling.IsInitCompleted)
                handling.HandleAllInitialize();
        }

        protected virtual void OnRemovedChildControl(AbstractControlContainer<TControl> sender, ControlEventArgs<TControl> e)
        {
            CursorContext[] hoverContexts = e.Control.GetHoverCursors();
            foreach (var hoverContext in hoverContexts)
                e.Control.HandleCursorMove(new(new(-1024, -1024), hoverContext));
        }

        public abstract IReadOnlyControlCollection<TControl> GetChildControls();

        IReadOnlyControlCollection<IControl> IContainerControl.GetChildControls()
        {
            return GetChildControls();
        }

        public override void HandleInitialize()
        {
            base.HandleInitialize();

            foreach (var control in GetChildControls())
            {
                control.HandleInitialize();
            }
        }

        public override void HandleInitCompleted1()
        {
            base.HandleInitCompleted1();

            foreach (var control in GetChildControls())
            {
                control.HandleInitCompleted1();
            }
        }

        public override void HandleInitCompleted2()
        {
            base.HandleInitCompleted2();

            foreach (var control in GetChildControls())
            {
                control.HandleInitCompleted2();
            }
        }

        public override void HandleInitCompleted3()
        {
            base.HandleInitCompleted3();

            foreach (var control in GetChildControls())
            {
                control.HandleInitCompleted3();
            }
        }

        public override void HandleCursorMove(CursorEventArgs e)
        {
            foreach (var control in GetChildControls().ToArray())
            {
                control.HandleCursorMove(e.Clone(control.ParentPos2ChildPos));
            }

            base.HandleCursorMove(e);
        }

        public override bool HandleRightClick(CursorEventArgs e)
        {
            TControl? control = GetChildControls().HoverControlOf(e.CursorContext);
            control?.HandleRightClick(e.Clone(control.ParentPos2ChildPos));

            return TryInvokeRightClick(e);
        }

        public override bool HandleLeftClick(CursorEventArgs e)
        {
            TControl? control = GetChildControls().HoverControlOf(e.CursorContext);
            control?.HandleLeftClick(e.Clone(control.ParentPos2ChildPos));

            return TryInvokeLeftClick(e);
        }

        public override bool HandleTextEditorUpdate(CursorEventArgs e)
        {
            TControl? control = GetChildControls().HoverControlOf(e.CursorContext);
            control?.HandleTextEditorUpdate(e.Clone(control.ParentPos2ChildPos));

            return TryInvokeTextEditorUpdate(e);
        }

        public override bool HandleCursorSlotChanged(CursorEventArgs e)
        {
            TControl? control = GetChildControls().HoverControlOf(e.CursorContext);
            control?.HandleCursorSlotChanged(e.Clone(control.ParentPos2ChildPos));

            return TryInvokeCursorSlotChanged(e);
        }

        public override bool HandleCursorItemChanged(CursorEventArgs e)
        {
            TControl? control = GetChildControls().HoverControlOf(e.CursorContext);
            control?.HandleCursorItemChanged(e.Clone(control.ParentPos2ChildPos));

            return TryInvokeCursorItemChanged(e);
        }

        public override void HandleBeforeFrame(EventArgs e)
        {
            foreach (var control in GetChildControls().ToArray())
            {
                control.HandleBeforeFrame(e);
            }

            base.HandleBeforeFrame(e);
        }

        public override void HandleAfterFrame(EventArgs e)
        {
            foreach (var control in GetChildControls().ToArray())
            {
                control.HandleAfterFrame(e);
            }

            base.HandleAfterFrame(e);
        }

        public override async Task<BlockFrame> GetRenderingResultAsync()
        {
            Task<BlockFrame> baseTask = base.GetRenderingResultAsync();

            List<IControl> childControls = new();
            List<Task<BlockFrame>> childTasks = new();
            foreach (var control in GetChildControls())
            {
                if (control.Visible && control.ClientSize.Width > 0 && control.ClientSize.Height > 0)
                {
                    childControls.Add(control);
                    childTasks.Add(control.GetRenderingResultAsync());
                }
            }

            BlockFrame baseFrame = await baseTask;
            BlockFrame[] childFrames = await Task.WhenAll(childTasks);

            await Task.Run(() =>
            {
                Foreach.Start(childControls, childFrames, (control, frame) =>
                {
                    baseFrame.Overwrite(frame, control.ClientSize, control.GetRenderingLocation(), control.OffsetPosition);
                    baseFrame.DrawBorder(control, control.GetRenderingLocation());
                });
            });

            return baseFrame;
        }
    }
}
