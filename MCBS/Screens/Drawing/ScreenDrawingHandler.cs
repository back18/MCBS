using MCBS.Cursor.Style;
using MCBS.Cursor;
using MCBS.Drawing;
using MCBS.Drawing.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using MCBS.UI.Extensions;
using QuanLib.Minecraft.Blocks;

namespace MCBS.Screens.Drawing
{
    public class ScreenDrawingHandler
    {
        private const string AIR_BLOCK = "minecraft:air";

        public ScreenDrawingHandler(ScreenContext owner)
        {
            ArgumentNullException.ThrowIfNull(owner, nameof(owner));

            _owner = owner;
            _drawingContext = new(
                new HashBlockFrame(1, 1, AIR_BLOCK),
                new HashBlockFrame(1, 1, AIR_BLOCK),
                new Dictionary<string, CursorDrawingContext>());
        }

        private readonly ScreenContext _owner;

        private ScreenDrawingContext _drawingContext;

        public async Task<ScreenDrawingContext> HandleFrameDrawingAsync()
        {
            return await Task.Run(() => HandleFrameDrawing());
        }

        public ScreenDrawingContext HandleFrameDrawing()
        {
            Dictionary<string, CursorDrawingContext> cursorDrawingContexts = [];
            foreach (var cursorContext in _owner.GetCursors())
            {
                if (cursorContext.ScreenContextOf != _owner)
                    continue;

                cursorDrawingContexts.Add(cursorContext.PlayerName, new(
                    cursorContext.NewInputData.CursorPosition,
                    cursorContext.StyleType,
                    cursorContext.Visible,
                    cursorContext.HoverControls.Values.ToArray()));
            }

            BlockFrame baseLayer;
            if (_owner.ScreenView.IsRequestRedraw ||
                _owner.RootForm.IsRequestRedraw ||
                _owner.RootForm.GetVisibleChildControls(true).Any(i => i.IsRequestRedraw))
            {
                baseLayer = _owner.ScreenView.GetDrawResult(true).BlockFrame;
            }
            else
            {
                baseLayer = _drawingContext.BaseLayer;
            }

            BlockFrame cursorLayer;
            if (_drawingContext.CursorLayer.Width == baseLayer.Width &&
                _drawingContext.CursorLayer.Height == baseLayer.Height &&
                !cursorDrawingContexts.Values.Where(w => w.IsRequestRedraw).Any() &&
                Equals(_drawingContext.CursorDrawingContexts, cursorDrawingContexts))
            {
                cursorLayer = _drawingContext.CursorLayer;
            }
            else
            {
                if (cursorDrawingContexts.Count == 0)
                {
                    cursorLayer = new HashBlockFrame(baseLayer.Width, baseLayer.Height, AIR_BLOCK);
                }
                else if (cursorDrawingContexts.Count == 1 && cursorDrawingContexts.First().Value.HoverControls.Count == 0)
                {
                    CursorDrawingContext cursorDrawingContext = cursorDrawingContexts.First().Value;

                    if (cursorDrawingContext.Visible)
                    {
                        Point position = cursorDrawingContext.CursorPosition;
                        if (!SR.CursorStyleManager.TryGetValue(cursorDrawingContext.StyleType, out var cursorStyle))
                            cursorStyle = SR.CursorStyleManager[CursorStyleType.Default];

                        Point offset = cursorStyle.Offset;
                        cursorLayer = new NestingBlockFrame(
                            baseLayer.Width,
                            baseLayer.Height,
                            cursorStyle.BlockFrame,
                            new(position.X - offset.X, position.Y - offset.Y),
                            Point.Empty,
                            0,
                            AIR_BLOCK,
                            string.Empty);
                    }
                    else
                    {
                        cursorLayer = new HashBlockFrame(baseLayer.Width, baseLayer.Height, AIR_BLOCK);
                    }
                }
                else
                {
                    LayerManager layerManager = new(baseLayer.Width, baseLayer.Height, AIR_BLOCK);
                    foreach (CursorDrawingContext cursorDrawingContext in cursorDrawingContexts.Values)
                    {
                        Point position = cursorDrawingContext.CursorPosition;

                        foreach (HoverControl hoverControl in cursorDrawingContext.HoverControls)
                        {
                            BlockFrame hoverFrame = hoverControl.Control.GetDrawResult(true).BlockFrame;
                            int borderWidth = hoverControl.Control.BorderWidth;

                            Point offset = hoverControl.OffsetPosition;
                            NestingBlockFrame layer = new(
                                layerManager.Width,
                                layerManager.Height,
                                hoverFrame,
                                new(position.X - offset.X, position.Y - offset.Y),
                                Point.Empty,
                                borderWidth,
                                string.Empty,
                                hoverControl.Control.GetBorderColor().ToBlockId());

                            layerManager.AddLayer(layer);
                        }

                        if (cursorDrawingContext.Visible)
                        {
                            if (!SR.CursorStyleManager.TryGetValue(cursorDrawingContext.StyleType, out var cursorStyle))
                                cursorStyle = SR.CursorStyleManager[CursorStyleType.Default];

                            Point offset = cursorStyle.Offset;
                            NestingBlockFrame layer = new(
                                layerManager.Width,
                                layerManager.Height,
                                cursorStyle.BlockFrame,
                                new(position.X - offset.X, position.Y - offset.Y),
                                Point.Empty,
                                0,
                                string.Empty,
                                string.Empty);

                            layerManager.AddLayer(layer);
                        }
                    }

                    cursorLayer = layerManager.AsBlockFrame();
                }
            }

            _drawingContext = new(baseLayer, cursorLayer, cursorDrawingContexts);
            return _drawingContext;
        }

        private static bool Equals(IReadOnlyDictionary<string, CursorDrawingContext> cursorDrawingContexts1, IReadOnlyDictionary<string, CursorDrawingContext> cursorDrawingContexts2)
        {
            ArgumentNullException.ThrowIfNull(cursorDrawingContexts1, nameof(cursorDrawingContexts1));
            ArgumentNullException.ThrowIfNull(cursorDrawingContexts2, nameof(cursorDrawingContexts2));

            if (cursorDrawingContexts1.Count != cursorDrawingContexts2.Count)
                return false;

            foreach (string key in cursorDrawingContexts1.Keys)
            {
                if (!cursorDrawingContexts1.TryGetValue(key, out var cursorDrawingContext1) || !cursorDrawingContexts2.TryGetValue(key, out var cursorDrawingContext2))
                    return false;

                if (!cursorDrawingContext1.Equals(cursorDrawingContext2))
                    return false;
            }

            return true;
        }
    }
}
