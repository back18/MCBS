using MCBS.Cursor;
using MCBS.Events;
using QuanLib.Minecraft.Blocks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.BlockForms
{
    public class DrawingBox : ScalablePictureBox
    {
        public DrawingBox()
        {
            EnableDrag = false;
            _PenWidth = 1;

            _drawingCursors = new();
            _undos = new();
            _redos = new();
        }

        private readonly List<CursorContext> _drawingCursors;

        private readonly Stack<Image<Rgba32>> _undos;

        private readonly Stack<Image<Rgba32>> _redos;

        public bool EnableDraw { get; set; }

        public int PenWidth
        {
            get => _PenWidth;
            set
            {
                if (value < 1)
                    value = 1;
                _PenWidth = value;
            }
        }
        private int _PenWidth;

        protected override void OnRightClick(Control sender, CursorEventArgs e)
        {
            base.OnRightClick(sender, e);

            if (EnableDraw)
            {
                if (_drawingCursors.Contains(e.CursorContext))
                {
                    _drawingCursors.Remove(e.CursorContext);
                }
                else
                {
                    _drawingCursors.Add(e.CursorContext);
                    _undos.Push(ImageFrame.Image.Clone());
                    ClearRedoStack();
                }
            }
        }

        protected override void OnCursorMove(Control sender, CursorEventArgs e)
        {
            base.OnCursorMove(sender, e);

            if (!_drawingCursors.Contains(e.CursorContext))
                return;

            Rgba32 color = GetCurrentColorOrDefault(e, BlockManager.Concrete.Black);
            Point position1 = ClientPos2ImagePos(new(e.Position.X - e.CursorPositionOffset.X, e.Position.Y - e.CursorPositionOffset.Y));
            Point position2 = ClientPos2ImagePos(e.Position);

            if (PixelMode && PenWidth == 1)
            {
                ImageFrame.Image[position2.X, position2.Y] = color;
            }
            else
            {
                PenOptions options = new(color, PenWidth);
                options.JointStyle = JointStyle.Round;
                options.EndCapStyle = EndCapStyle.Round;
                SolidPen pen = new(options);

                ImageFrame.Image.Mutate(ctx =>
                {
                    ctx.DrawLine(pen, new PointF[] { position1, position2 });
                });
            }

            ImageFrame.Update(Rectangle);
            RequestUpdateFrame();
        }

        protected override void OnCursorEnter(Control sender, CursorEventArgs e)
        {
            base.OnCursorEnter(sender, e);

            e.CursorContext.Visible = false;
            if (_drawingCursors.Contains(e.CursorContext))
                _drawingCursors.Remove(e.CursorContext);
        }

        protected override void OnCursorLeave(Control sender, CursorEventArgs e)
        {
            base.OnCursorLeave(sender, e);

            e.CursorContext.Visible = true;
        }

        protected override void OnImageFrameChanged(PictureBox sender, ImageFrameChangedEventArgs e)
        {
            base.OnImageFrameChanged(sender, e);

            ClearUndoStack();
            ClearRedoStack();
        }

        public void Clear()
        {
            Fill(BlockManager.Concrete.White);
        }

        public void Fill(CursorEventArgs e)
        {
            Fill(GetCurrentColorOrDefault(e, BlockManager.Concrete.White));
        }

        public void Fill(string blockID)
        {
            Fill(GetBlockColor(blockID));
        }

        public void Fill(Rgba32 color)
        {
            _undos.Push(ImageFrame.Image.Clone());
            ClearRedoStack();

            ImageFrame.Image.Mutate(ctx => ctx.BackgroundColor(color).Fill(color));
            ImageFrame.Update(Rectangle);
            RequestUpdateFrame();
        }

        public void Undo()
        {
            if (_undos.Count > 0)
            {
                _redos.Push(ImageFrame.Image);
                ImageFrame.Image = _undos.Pop();
                ImageFrame.Update(Rectangle);
                RequestUpdateFrame();
            }
        }

        public void Redo()
        {
            if (_redos.Count > 0)
            {
                _undos.Push(ImageFrame.Image);
                ImageFrame.Image = _redos.Pop();
                ImageFrame.Update(Rectangle);
                RequestUpdateFrame();
            }
        }

        private void ClearUndoStack()
        {
            while (_undos.Count > 0)
            {
                _undos.Pop().Dispose();
            }
        }

        private void ClearRedoStack()
        {
            while (_redos.Count > 0)
            {
                _redos.Pop().Dispose();
            }
        }

        private Rgba32 GetCurrentColorOrDefault(CursorEventArgs e, string def)
        {
            return GetCurrentColorOrDefault(e, GetBlockColor(def));
        }

        private Rgba32 GetCurrentColorOrDefault(CursorEventArgs e, Rgba32 def)
        {
            var id = e.CursorContext.NewInputData.DeputyItem?.ID;
            return GetBlockColorOrDefault(id, def);
        }
    }
}
