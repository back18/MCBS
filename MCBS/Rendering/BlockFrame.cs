using QuanLib.Core;
using SixLabors.ImageSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Rendering
{
    public abstract class BlockFrame : IPixelCollection<string>
    {
        public abstract string this[int index] { get; set; }

        public abstract string this[int x, int y] { get; set; }

        public abstract int Count { get; }

        public abstract int Width { get; }

        public abstract int Height { get; }

        public abstract SearchMode SearchMode { get; }

        public virtual bool SupportTransparent => true;

        public virtual string TransparentPixel => string.Empty;

        public static IDictionary<Point, string> GetDifferencesPixel(BlockFrame blockFrame1, BlockFrame blockFrame2)
        {
            if (blockFrame1 is null)
                throw new ArgumentNullException(nameof(blockFrame1));
            if (blockFrame2 is null)
                throw new ArgumentNullException(nameof(blockFrame2));
            if (blockFrame1.Width != blockFrame1.Width || blockFrame1.Height != blockFrame2.Height)
                throw new ArgumentException("帧尺寸不一致");

            PositionEnumerable positions = new(blockFrame1.Width, blockFrame1.Height);

            Dictionary<Point, string> result = new();
            Foreach.Start(positions, blockFrame1, blockFrame2, (position, pixel1, pixel2) =>
            {
                if (pixel1 != pixel2)
                    result.Add(position, pixel2);
            });

            return result;
        }

        public virtual OverwriteContext Overwrite(BlockFrame blockFrame, Point location)
        {
            if (blockFrame is null)
                throw new ArgumentNullException(nameof(blockFrame));

            return Overwrite(blockFrame.AsPixelCollection(), location);
        }

        public virtual OverwriteContext Overwrite(IPixelCollection<string> pixels, Point location)
        {
            if (pixels is null)
                throw new ArgumentNullException(nameof(pixels));

            OverwriteContext overwriteContext = new(new(Width, Height), new(pixels.Width, pixels.Height), location);
            if (pixels.SupportTransparent)
            {
                string transparent = pixels.TransparentPixel;
                foreach (var mapping in overwriteContext)
                {
                    string pixel = pixels[mapping.OverwritePosition.X, mapping.OverwritePosition.Y];
                    if (pixel != transparent)
                        this[mapping.BasePosition.X, mapping.BasePosition.Y] = pixel;
                }
            }
            else
            {
                foreach (var mapping in overwriteContext)
                {
                    this[mapping.BasePosition.X, mapping.BasePosition.Y] = pixels[mapping.OverwritePosition.X, mapping.OverwritePosition.Y];
                }
            }

            return overwriteContext;
        }

        public abstract void Fill(string pixel);

        public abstract IDictionary<Point, string> GetAllPixel();

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
