using MCBS.BlockForms.Utility;
using MCBS.Cursor;
using MCBS.Events;
using MCBS.Rendering;
using Newtonsoft.Json.Linq;
using QuanLib.Minecraft.Blocks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.BlockForms
{
    public class ScalablePictureBox<TPixel> : PictureBox<TPixel> where TPixel : unmanaged, IPixel<TPixel>
    {
        public ScalablePictureBox()
        {
            ContentAnchor = AnchorPosition.UpperLeft;

            FirstHandleCursorSlotChanged = true;
            DefaultResizeOptions.Mode = ResizeMode.Max;
            ScalingRatio = 0.2;
            EnableZoom = false;
            EnableDrag = false;

            PixelModeThreshold = 5;
            _draggingCursors = new();
        }

        private readonly List<CursorContext> _draggingCursors;

        public int PixelModeThreshold { get; set; }

        public bool PixelMode => GetPixelLength() >= PixelModeThreshold;

        public double ScalingRatio { get; set; }

        public bool EnableZoom { get; set; }

        public bool EnableDrag { get; set; }

        protected override BlockFrame Rendering()
        {
            int pixelLength = GetPixelLength();
            if (pixelLength >= PixelModeThreshold)
            {
                BlockFrame textureFrame = Texture.CreateBlockFrame(new(Texture.CropRectangle.Width * pixelLength, Texture.CropRectangle.Height * pixelLength), GetScreenPlane().NormalFacing);

                for (int x = textureFrame.Width - 1; x >= 0; x -= pixelLength)
                    textureFrame.DrawVerticalLine(x, BlockManager.Concrete.Gray);
                for (int y = textureFrame.Height - 1; y >= 0; y -= pixelLength)
                    textureFrame.DrawHorizontalLine(y, BlockManager.Concrete.Gray);

                BlockFrame baseFrame = this.RenderingBackground(ClientSize);
                baseFrame.Overwrite(textureFrame, Point.Empty);
                return baseFrame;
            }
            else
            {
                return base.Rendering();
            }
        }

        protected override void OnCursorMove(Control sender, CursorEventArgs e)
        {
            base.OnCursorMove(sender, e);

            if (!_draggingCursors.Contains(e.CursorContext))
                return;

            Point position1 = ClientPos2ImagePos(new(e.Position.X - e.CursorPositionOffset.X, e.Position.Y - e.CursorPositionOffset.Y));
            Point position2 = ClientPos2ImagePos(e.Position);
            Point offset = new(position2.X - position1.X, position2.Y - position1.Y);
            Rectangle rectangle = Texture.CropRectangle;
            rectangle.X -= offset.X;
            rectangle.Y -= offset.Y;
            CorrectAndUpdateCropRectangle(rectangle);
        }

        protected override void OnCursorLeave(Control sender, CursorEventArgs e)
        {
            base.OnCursorLeave(sender, e);

            if (_draggingCursors.Contains(e.CursorContext))
                _draggingCursors.Remove(e.CursorContext);
        }

        protected override void OnRightClick(Control sender, CursorEventArgs e)
        {
            base.OnRightClick(sender, e);

            if (EnableDrag)
            {
                if (_draggingCursors.Contains(e.CursorContext))
                    _draggingCursors.Remove(e.CursorContext);
                else
                    _draggingCursors.Add(e.CursorContext);
            }
        }

        protected override void OnResize(Control sender, SizeChangedEventArgs e)
        {
            Texture.CropRectangle = Texture.ImageSource.Bounds;
            UpdateTextureTexture();

            if (Texture.GetOutputSize() == e.NewSize)
                return;

            Texture.ResizeOptions.Size = e.NewSize;
            if (AutoSize)
                AutoSetSize();
        }

        protected override void OnTextureChanged(PictureBox<TPixel> sender, TextureChangedEventArgs<TPixel> e)
        {
            base.OnTextureChanged(sender, e);

            UpdateTextureTexture();
        }

        protected override void OnCursorSlotChanged(Control sender, CursorEventArgs e)
        {
            base.OnCursorSlotChanged(sender, e);

            if (!EnableZoom)
                return;

            Rectangle rectangle = Texture.CropRectangle;

            Point position1 = ClientPos2ImagePos(rectangle, e.Position);
            Point center1 = GetImageCenterPosition(rectangle);
            Point offset1 = new(position1.X - center1.X, position1.Y - center1.Y);

            rectangle.Width += (int)Math.Round(e.InventorySlotDelta * rectangle.Width * ScalingRatio);
            rectangle.Height += (int)Math.Round(e.InventorySlotDelta * rectangle.Height * ScalingRatio);
            rectangle.X = center1.X - (int)Math.Round(rectangle.Width / 2.0);
            rectangle.Y = center1.Y - (int)Math.Round(rectangle.Height / 2.0);

            Point position2 = ClientPos2ImagePos(rectangle, e.Position);
            Point center2 = GetImageCenterPosition(rectangle);
            Point offset2 = new(position2.X - center2.X, position2.Y - center2.Y);

            rectangle.X += offset1.X - offset2.X;
            rectangle.Y += offset1.Y - offset2.Y;

            CorrectAndUpdateCropRectangle(rectangle);
        }

        public Point GetImageCenter() => GetImageCenterPosition(Texture.CropRectangle);

        public Point ClientPos2ImagePos(Point position) => ClientPos2ImagePos(Texture.CropRectangle, position);

        public int GetPixelLength() => GetPixelLength(Texture.CropRectangle);

        private static Point GetImageCenterPosition(Rectangle rectangle)
        {
            return new(rectangle.X + (int)Math.Round(rectangle.Width / 2.0), rectangle.Y + (int)Math.Round(rectangle.Height / 2.0));
        }

        private Point ClientPos2ImagePos(Rectangle rectangle, Point position)
        {
            int pixel = GetPixelLength(rectangle);
            if (pixel >= PixelModeThreshold)
            {
                Point pxpos = new(rectangle.X + position.X / pixel, rectangle.Y + position.Y / pixel);
                pxpos.X = Math.Min(rectangle.X + rectangle.Width - 1, pxpos.X);
                pxpos.Y = Math.Min(rectangle.Y + rectangle.Height - 1, pxpos.Y);
                return pxpos;
            }
            else
            {
                double pixels = (double)rectangle.Width / ClientSize.Width;
                return new(rectangle.X + (int)Math.Round(position.X * pixels, MidpointRounding.ToNegativeInfinity), rectangle.Y + (int)Math.Round(position.Y * pixels, MidpointRounding.ToNegativeInfinity));
            }
        }

        private int GetPixelLength(Rectangle rectangle)
        {
            if (rectangle.Width == Texture.ImageSource.Width && rectangle.Height == Texture.ImageSource.Height)
            {
                int xpx = (int)Math.Round((double)ClientSize.Width / rectangle.Width, MidpointRounding.ToNegativeInfinity);
                int ypx = (int)Math.Round((double)ClientSize.Height / rectangle.Height, MidpointRounding.ToNegativeInfinity);
                return Math.Min(xpx, ypx);
            }
            else
            {
                int xpx = (int)Math.Round((double)ClientSize.Width / rectangle.Width, MidpointRounding.ToPositiveInfinity);
                int ypx = (int)Math.Round((double)ClientSize.Height / rectangle.Height, MidpointRounding.ToPositiveInfinity);
                return Math.Max(xpx, ypx);
            }
        }

        private void CorrectAndUpdateCropRectangle(Rectangle rectangle)
        {
            rectangle = CorrectCropRectangle(rectangle);

            Texture.CropRectangle = rectangle;
            UpdateTextureTexture();
            RequestRendering();
        }

        private Rectangle CorrectCropRectangle(Rectangle rectangle)
        {
            if (rectangle.Width < 1)
                rectangle.Width = 1;
            else if (rectangle.Width > Texture.ImageSource.Width)
                rectangle.Width = Texture.ImageSource.Width;
            if (rectangle.Height < 1)
                rectangle.Height = 1;
            else if (rectangle.Height > Texture.ImageSource.Height)
                rectangle.Height = Texture.ImageSource.Height;

            if (rectangle.X + rectangle.Width > Texture.ImageSource.Width - 1)
                rectangle.X = Texture.ImageSource.Width - rectangle.Width;
            if (rectangle.Y + rectangle.Height > Texture.ImageSource.Height - 1)
                rectangle.Y = Texture.ImageSource.Height - rectangle.Height;

            if (rectangle.X < 0)
                rectangle.X = 0;
            else if (rectangle.X > Texture.ImageSource.Width - 1)
                rectangle.X = Texture.ImageSource.Width - 1;
            if (rectangle.Y < 0)
                rectangle.Y = 0;
            else if (rectangle.Y > Texture.ImageSource.Height - 1)
                rectangle.Y = Texture.ImageSource.Height - 1;

            return rectangle;
        }

        private void UpdateTextureTexture()
        {
            if (Texture.CropRectangle.Width > ClientSize.Width)
                Texture.ResizeOptions.Sampler = KnownResamplers.Bicubic;
            else
                Texture.ResizeOptions.Sampler = KnownResamplers.NearestNeighbor;
        }
    }
}
