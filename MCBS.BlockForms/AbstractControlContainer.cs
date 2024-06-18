﻿using MCBS.Cursor;
using MCBS.Drawing;
using MCBS.Drawing.Extensions;
using MCBS.Events;
using MCBS.UI;
using MCBS.UI.Extensions;
using QuanLib.Core;
using QuanLib.Core.Events;
using System;
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

        public abstract event EventHandler<AbstractControlContainer<TControl>, EventArgs<TControl>> AddedChildControl;

        public abstract event EventHandler<AbstractControlContainer<TControl>, EventArgs<TControl>> RemovedChildControl;

        protected virtual void OnAddedChildControl(AbstractControlContainer<TControl> sender, EventArgs<TControl> e)
        {
            IControlInitializeHandling handling = e.Argument;
            if (IsInitCompleted && !handling.IsInitCompleted)
            {
                handling.HandleBeforeInitialize();
                handling.HandleInitialize();
                handling.HandleAfterInitialize();
            }
        }

        protected virtual void OnRemovedChildControl(AbstractControlContainer<TControl> sender, EventArgs<TControl> e)
        {
            CursorContext[] hoverContexts = e.Argument.GetHoverCursors();
            foreach (var hoverContext in hoverContexts)
                e.Argument.HandleCursorMove(new(new(-1024, -1024), hoverContext));
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

        public override void HandleBeforeInitialize()
        {
            base.HandleBeforeInitialize();

            foreach (var control in GetChildControls())
            {
                control.HandleBeforeInitialize();
            }
        }

        public override void HandleAfterInitialize()
        {
            base.HandleAfterInitialize();

            foreach (var control in GetChildControls())
            {
                control.HandleAfterInitialize();
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

        public override async Task<BlockFrame> GetDrawResultAsync()
        {
            Task<BlockFrame> baseTask = base.GetDrawResultAsync();

            List<IControl> childControls = new();
            List<Task<BlockFrame>> childTasks = new();
            foreach (var control in GetChildControls())
            {
                if (control.Visible && control.ClientSize.Width > 0 && control.ClientSize.Height > 0)
                {
                    childControls.Add(control);
                    childTasks.Add(control.GetDrawResultAsync());
                }
            }

            BlockFrame baseFrame = await baseTask;
            BlockFrame[] childFrames = await Task.WhenAll(childTasks);

            await Task.Run(() =>
            {
                Foreach.Start(childControls, childFrames, (control, frame) =>
                {
                    baseFrame.Overwrite(frame, control.ClientSize, control.GetDrawingLocation(), control.OffsetPosition);
                    baseFrame.DrawBorder(control, control.GetDrawingLocation());
                });
            });

            return baseFrame;
        }
    }
}
