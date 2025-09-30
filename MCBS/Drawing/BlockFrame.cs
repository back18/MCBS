using QuanLib.Core;
using SixLabors.ImageSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Drawing
{
    public abstract class BlockFrame : IPixelCollection<string>
    {
        protected BlockFrame()
        {
            ContainsTransparent = true;
        }

        public abstract string this[int index] { get; set; }

        public abstract string this[int x, int y] { get; set; }

        public abstract int Count { get; }

        public abstract int Width { get; }

        public abstract int Height { get; }

        public abstract SearchMode SearchMode { get; }

        public virtual bool SupportTransparent => true;

        public virtual bool ContainsTransparent { get; set; }

        public virtual string TransparentPixel => string.Empty;

        public static ScreenPixel<string>[] GetDifferencePixels(BlockFrame blockFrame1, BlockFrame blockFrame2)
        {
            ArgumentNullException.ThrowIfNull(blockFrame1, nameof(blockFrame1));
            ArgumentNullException.ThrowIfNull(blockFrame2, nameof(blockFrame2));
            if (blockFrame1.Width != blockFrame1.Width || blockFrame1.Height != blockFrame2.Height)
                throw new ArgumentException("帧尺寸不一致");

            PositionEnumerable positions = new(blockFrame1.Width, blockFrame1.Height);

            List<ScreenPixel<string>> result = [];
            Foreach.Start(positions, blockFrame1, blockFrame2, (position, pixel1, pixel2) =>
            {
                if (pixel1 != pixel2)
                    result.Add(new(position, pixel2));
            });

            return result.ToArray();
        }

        public virtual bool IsTransparentPixel(int index)
        {
            if (!SupportTransparent)
                return false;

            return this[index] == TransparentPixel;
        }

        public virtual bool IsTransparentPixel(int x, int y)
        {
            if (!SupportTransparent)
                return false;

            return this[x, y] == TransparentPixel;
        }

        public virtual bool CheckTransparentPixel()
        {
            if (!SupportTransparent)
                return false;

            foreach (string block in this)
            {
                if (block == TransparentPixel)
                    return true;
            }

            return false;
        }

        public virtual OverwriteContext Overwrite(BlockFrame blockFrame, Size size, Point location, Point offset)
        {
            ArgumentNullException.ThrowIfNull(blockFrame, nameof(blockFrame));

            return Overwrite(blockFrame.AsPixelCollection(), size, location, offset);
        }

        public OverwriteContext Overwrite(IPixelCollection<string> pixels, Size size, Point location, Point offset)
        {
            ArgumentNullException.ThrowIfNull(pixels, nameof(pixels));

            OverwriteContext overwriteContext = new(new(Width, Height), new(pixels.Width, pixels.Height), new(size.Width, size.Height), location, offset);
            if (pixels.SupportTransparent)
            {
                Parallel.ForEach(overwriteContext, (mapping) =>
                {
                    Point overwritePosition = mapping.OverwritePosition;
                    if (!pixels.IsTransparentPixel(overwritePosition.X, overwritePosition.Y))
                        this[mapping.BasePosition.X, mapping.BasePosition.Y] = pixels[overwritePosition.X, overwritePosition.Y];
                });
            }
            else
            {
                Parallel.ForEach(overwriteContext, (mapping) => this[mapping.BasePosition.X, mapping.BasePosition.Y] = pixels[mapping.OverwritePosition.X, mapping.OverwritePosition.Y]);
            }

            return overwriteContext;
        }

        public virtual BlockFrame Crop(Rectangle rectangle)
        {
            if (rectangle.X < 0)
            {
                rectangle.Width += rectangle.X;
                rectangle.X = 0;
            }
            if (rectangle.Y < 0)
            {
                rectangle.Height += rectangle.Y;
                rectangle.Y = 0;
            }

            rectangle.Width = Math.Min(rectangle.Width, Width - rectangle.X);
            rectangle.Height = Math.Min(rectangle.Height, Height - rectangle.Y);

            int count = rectangle.Width * rectangle.Height;
            if (count <= 0)
                throw new InvalidOperationException();

            LosslessBlockFrame losslessBlockFrame = new(rectangle.Width, rectangle.Height);
            for (int y1 = rectangle.Y, y2 = 0; y1 < rectangle.Height; y1++, y2++)
                for (int x1 = rectangle.X, x2 = 0; x1 < rectangle.Width; x1++, x2++)
                    losslessBlockFrame[x2, y2] = this[x1, y1];

            return losslessBlockFrame;
        }

        public abstract BlockFrame Clone();

        public abstract void Fill(string pixel);

        public abstract ScreenPixel<string>[] GetAllPixel();

        public abstract string[] ToArray();

        public virtual void CopyPixelDataTo(Span<string> destination)
        {
            ToArray().CopyTo(destination);
        }

        public virtual IPixelCollection<string> AsPixelCollection() => this;

        public abstract IEnumerator<string> GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
