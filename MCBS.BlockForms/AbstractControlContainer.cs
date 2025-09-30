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

        private BlockFrame? _drawCache;

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

        public override DrawResult GetDrawResult(bool drawChildControls)
        {
            if (!drawChildControls)
                return base.GetDrawResult(drawChildControls);

            IControl[] childControls = this.GetVisibleChildControls(false);

            if (childControls.Length == 0)
                return base.GetDrawResult(drawChildControls);

            if (this.GetVisibleChildControls(true).Any(w => w.IsRequestRedraw))
            {
                if (_drawCache is not LayerBlockFrame)
                    RequestRedraw();
            }
            else
            {
                if (!IsRequestRedraw && _drawCache is not null)
                    return new(this, _drawCache, false, TimeSpan.Zero);
            }

            Stopwatch stopwatch = Stopwatch.StartNew();

            Task<DrawResult> drawingTask = Task.Run(() => base.GetDrawResult(drawChildControls));
            ConcurrentBag<DrawResult> childDrawResultBag = [];

            Stopwatch stopwatch1 = Stopwatch.StartNew();
            Parallel.ForEach(childControls, control => childDrawResultBag.Add(control.GetDrawResult(drawChildControls)));
            stopwatch1.Stop();

            Stopwatch stopwatch2 = Stopwatch.StartNew();
            DrawResult drawResult = drawingTask.Result;
            stopwatch2.Stop();

            List<DrawResult> childDrawResults = [];
            foreach (var control in childControls)
            {
                DrawResult? childDrawResult = childDrawResultBag.FirstOrDefault(f => f.Control == control);
                if (childDrawResult is not null)
                    childDrawResults.Add(childDrawResult);
            }

            Stopwatch stopwatch3 = Stopwatch.StartNew();
            BlockFrame background = drawResult.BlockFrame;
            int pixelTotal = background.Width * background.Height;
            int pixelCount = childDrawResultBag.Sum(i => i.BlockFrame.Width * i.BlockFrame.Height);
            _drawCache = pixelCount > pixelTotal / 2 ? MultiLayerOverwrite(background, GetBackgroundColor().ToBlockId(), childDrawResults) : SingleLayerOverwrite(background, childDrawResults);
            stopwatch3.Stop();

            stopwatch.Stop();

            return new ContainerDrawResult(
                this,
                _drawCache,
                true,
                stopwatch.Elapsed,
                stopwatch2.Elapsed,
                stopwatch1.Elapsed,
                stopwatch3.Elapsed,
                childDrawResultBag.ToArray());
        }

        private static BlockFrame SingleLayerOverwrite(BlockFrame background, IList<DrawResult> childDrawResults)
        {
            BlockFrame blockFrame = background.Clone();
            foreach (var childDrawResult in childDrawResults)
            {
                IControl control = childDrawResult.Control;
                blockFrame.Overwrite(childDrawResult.BlockFrame, control.ClientSize, control.GetDrawingLocation(), control.OffsetPosition);
                blockFrame.DrawBorder(control, control.GetDrawingLocation());
            }
            return blockFrame;
        }

        private static BlockFrame MultiLayerOverwrite(BlockFrame background, string backgroundColor, IList<DrawResult> childDrawResults)
        {
            LayerManager layerManager = new(background.Width, background.Height, backgroundColor);
            layerManager.AddLayer(background);

            foreach (var childDrawResult in childDrawResults)
            {
                IControl control = childDrawResult.Control;
                Point location = control.GetDrawingLocation();

                BlockFrame blockFrame;
                if (childDrawResult.Control.BorderWidth == 0 &&
                    childDrawResult.BlockFrame.Width == background.Width &&
                    childDrawResult.BlockFrame.Height == background.Height)
                    blockFrame = childDrawResult.BlockFrame;
                else
                    blockFrame = new NestingBlockFrame(
                        background.Width,
                        background.Height,
                        childDrawResult.BlockFrame,
                        location,
                        control.OffsetPosition,
                        control.BorderWidth,
                        string.Empty,
                        control.GetBorderColor().ToBlockId());

                layerManager.AddLayer(blockFrame);
            }

            return layerManager.AsBlockFrame();
        }
    }
}
