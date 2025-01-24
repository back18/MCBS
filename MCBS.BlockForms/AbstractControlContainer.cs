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
using System.Diagnostics;
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
            Stopwatch stopwatch = Stopwatch.StartNew();

            TControl[] ChildControls =
                GetChildControls()
                .Where(w =>
                w.Visible &&
                w.ClientSize.Width > 0 &&
                w.ClientSize.Height > 0 &&
                w.ClientLocation.X + w.ClientSize.Width + w.BorderWidth > OffsetPosition.X &&
                w.ClientLocation.Y + w.ClientSize.Height + w.BorderWidth > OffsetPosition.Y &&
                w.ClientLocation.X - w.BorderWidth < ClientSize.Width + OffsetPosition.X &&
                w.ClientLocation.Y - w.BorderWidth < ClientSize.Height + OffsetPosition.Y)
                .ToArray();

            if (ChildControls.Length == 0)
                return base.GetDrawResult();

            Task<DrawResult> drawingTask = Task.Run(() => base.GetDrawResult());
            ConcurrentBag<DrawResult> childDrawResults = [];

            Stopwatch stopwatch1 = Stopwatch.StartNew();
            Parallel.ForEach(ChildControls, control => childDrawResults.Add(control.GetDrawResult()));
            stopwatch1.Stop();

            Stopwatch stopwatch2 = Stopwatch.StartNew();
            DrawResult drawResult = drawingTask.Result;
            stopwatch2.Stop();

            if (!drawResult.IsRedraw && !childDrawResults.Any(w => w.IsRedraw))
                return drawResult;

            Stopwatch stopwatch3 = Stopwatch.StartNew();
            foreach (var control in ChildControls)
            {
                DrawResult? childDrawResult = childDrawResults.FirstOrDefault(f => f.Control == control);
                if (childDrawResult is null)
                    continue;

                BlockFrame background = drawResult.BlockFrame;
                background.Overwrite(childDrawResult.BlockFrame, control.ClientSize, control.GetDrawingLocation(), control.OffsetPosition);
                background.DrawBorder(control, control.GetDrawingLocation());
            }
            stopwatch3.Stop();

            stopwatch.Stop();

            return new ContainerDrawResult(
                this,
                drawResult.BlockFrame,
                true,
                stopwatch.Elapsed,
                stopwatch2.Elapsed,
                stopwatch1.Elapsed,
                stopwatch3.Elapsed,
                childDrawResults.ToArray());
        }
    }
}
