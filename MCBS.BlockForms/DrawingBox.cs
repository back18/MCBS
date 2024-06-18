using MCBS.BlockForms.Utility;
using MCBS.Cursor;
using MCBS.Drawing;
using MCBS.Events;
using QuanLib.Core.Events;
using QuanLib.Minecraft.Blocks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.BlockForms
{
    public class DrawingBox<TPixel> : ScalablePictureBox<TPixel> where TPixel : unmanaged, IPixel<TPixel>
    {
        public DrawingBox()
        {
            EnableDrag = false;
            RequestDrawTransparencyTexture = false;
            Skin.SetAllBackgroundColor("minecraft:glass");
            _PenWidth = 1;

            _drawingCursors = new();
            _undos = new();
            _redos = new();
        }

        private readonly List<CursorContext> _drawingCursors;

        private readonly Stack<Image<TPixel>> _undos;

        private readonly Stack<Image<TPixel>> _redos;

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
                    _undos.Push(Texture.ImageSource.Clone());
                    ClearRedoStack();
                }
            }
        }

        protected override void OnCursorMove(Control sender, CursorEventArgs e)
        {
            base.OnCursorMove(sender, e);

            if (!_drawingCursors.Contains(e.CursorContext))
                return;

            TPixel color = GetCurrentColorOrDefault(e, BlockManager.Concrete.Black);
            Point position1 = ClientPos2ImagePos(new(e.Position.X - e.CursorPositionOffset.X, e.Position.Y - e.CursorPositionOffset.Y));
            Point position2 = ClientPos2ImagePos(e.Position);

            if (PixelMode && PenWidth == 1)
            {
                Texture.ImageSource[position2.X, position2.Y] = color;
            }
            else
            {
                PenOptions options = new(Color.FromPixel(color), PenWidth);
                options.JointStyle = JointStyle.Round;
                options.EndCapStyle = EndCapStyle.Round;
                SolidPen pen = new(options);

                Texture.ImageSource.Mutate(ctx =>
                {
                    ctx.DrawLine(pen, new PointF[] { position1, position2 });
                });
            }

            Texture.ImageSourceUpdated();
            RequestRedraw();
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

        protected override void OnTextureChanged(PictureBox<TPixel> sender, ValueChangedEventArgs<Texture<TPixel>> e)
        {
            base.OnTextureChanged(sender, e);

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

        public void Fill(string blockId)
        {
            Fill(this.GetBlockColor<TPixel>(blockId));
        }

        public void Fill(TPixel color)
        {
            _undos.Push(Texture.ImageSource.Clone());
            ClearRedoStack();

            Texture.ImageSource.Mutate(ctx => ctx.Fill(Color.FromPixel(color)));
            Texture.ImageSourceUpdated();
            RequestRedraw();
        }

        public void Undo()
        {
            if (_undos.Count > 0)
            {
                _redos.Push(Texture.ImageSource);
                Texture.ImageSourceUpdated(_undos.Pop(), false);
                RequestRedraw();
            }
        }

        public void Redo()
        {
            if (_redos.Count > 0)
            {
                _undos.Push(Texture.ImageSource);
                Texture.ImageSourceUpdated(_redos.Pop(), false);
                RequestRedraw();
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

        private TPixel GetCurrentColorOrDefault(CursorEventArgs e, string defaultBlockId)
        {
            return GetCurrentColorOrDefault(e, this.GetBlockColor<TPixel>(defaultBlockId));
        }

        private TPixel GetCurrentColorOrDefault(CursorEventArgs e, TPixel defaultColor)
        {
            var id = e.CursorContext.NewInputData.MainItem?.ID;
            return this.GetBlockColorOrDefault<TPixel>(id, defaultColor);
        }
    }
}
