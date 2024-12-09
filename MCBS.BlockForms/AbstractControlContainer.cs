using MCBS.Cursor;
using MCBS.Drawing;
using MCBS.Drawing.Extensions;
using MCBS.Events;
using MCBS.UI;
using MCBS.UI.Extensions;
using QuanLib.Core;
using QuanLib.Core.Events;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Concurrent;
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

        public override bool HandleCursorMove(CursorEventArgs e)
        {
            foreach (var control in GetChildControls().ToArray())
            {
                control.HandleCursorMove(e.Clone(control.ParentPos2ChildPos));
            }

            return base.HandleCursorMove(e);
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

        public override DrawResult GetDrawResult()
        {
            Task<DrawResult> drawingTask = Task.Run(() => base.GetDrawResult());
            ConcurrentBag<DrawResult> childDrawResults = [];
            IEnumerable<TControl> ChildControls =
                GetChildControls()
                .Where(w =>
                w.Visible &&
                w.ClientSize.Width > 0 &&
                w.ClientSize.Height > 0);

            Parallel.ForEach(ChildControls, control => childDrawResults.Add(control.GetDrawResult()));
            DrawResult drawResult = drawingTask.Result;

            if (!drawResult.IsRedraw && !childDrawResults.Where(w => w.IsRedraw).Any())
                return drawResult;

            foreach (var control in ChildControls)
            {
                DrawResult? drawResult1 = childDrawResults.FirstOrDefault(f => f.Control == control);
                if (drawResult1 is null)
                    continue;

                drawResult.BlockFrame.Overwrite(drawResult1.BlockFrame, drawResult1.Control.ClientSize, drawResult1.Control.GetDrawingLocation(), drawResult1.Control.OffsetPosition);
                drawResult.BlockFrame.DrawBorder(drawResult1.Control, drawResult1.Control.GetDrawingLocation());
            }

            return drawResult;
        }
    }
}
