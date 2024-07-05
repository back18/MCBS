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

namespace MCBS.Screens.Drawing
{
    public class ScreenDrawingHandler
    {
        private const string AIR_BLOCK = "minecraft:air";

        public ScreenDrawingHandler(ScreenContext owner)
        {
            ArgumentNullException.ThrowIfNull(owner, nameof(owner));

            _owner = owner;
            _drawingContext = new(new HashBlockFrame(owner.Screen.Width, owner.Screen.Height, AIR_BLOCK), new Dictionary<string, CursorDrawingContext>());
        }

        private readonly ScreenContext _owner;

        private ScreenDrawingContext _drawingContext;

        public async Task<BlockFrame> HandleFrameDrawingAsync()
        {
            return await Task.Run(() => HandleFrameDrawing());
        }

        public BlockFrame HandleFrameDrawing()
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

            if (Equals(_drawingContext.CursorDrawingContexts, cursorDrawingContexts) &&
                !cursorDrawingContexts.Values.Where(w => w.IsRequestRedraw).Any() &&
                !_owner.ScreenView.IsRequestRedraw)
                return _drawingContext.BlockFrame;

            DrawResult drawResult = _owner.ScreenView.GetDrawResult();
            BlockFrame blockFrame = drawResult.BlockFrame.Clone();

            foreach (CursorDrawingContext cursorDrawingContext in cursorDrawingContexts.Values)
            {
                Point position = cursorDrawingContext.CursorPosition;

                foreach (HoverControl hoverControl in cursorDrawingContext.HoverControls)
                {
                    BlockFrame hoverFrame = hoverControl.Control.GetDrawResult().BlockFrame;
                    Point offset = hoverControl.OffsetPosition;
                    blockFrame.Overwrite(hoverFrame, hoverControl.Control.ClientSize, new(position.X - offset.X, position.Y - offset.Y));
                    blockFrame.DrawBorder(hoverControl.Control, position, hoverControl.OffsetPosition);
                }

                if (cursorDrawingContext.Visible)
                {
                    if (!SR.CursorStyleManager.TryGetValue(cursorDrawingContext.StyleType, out var cursorStyle))
                        cursorStyle = SR.CursorStyleManager[CursorStyleType.Default];
                    Point offset = cursorStyle.Offset;
                    blockFrame.Overwrite(cursorStyle.BlockFrame, new(position.X - offset.X, position.Y - offset.Y));
                }
            }

            _drawingContext = new(blockFrame, cursorDrawingContexts);
            return _drawingContext.BlockFrame;
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
