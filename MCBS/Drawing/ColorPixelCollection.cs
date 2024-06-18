using QuanLib.Core;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Drawing
{
    public class ColorPixelCollection<TPixel> : UnmanagedBase, IPixelCollection<TPixel> where TPixel : unmanaged, IPixel<TPixel>
    {
        static ColorPixelCollection()
        {
            _transparentPixelTypes = new()
            {
                typeof(Rgba32),
                typeof(Rgba64),
                typeof(Rgba1010102),
                typeof(Argb32),
                typeof(Abgr32),
                typeof(Bgra32),
                typeof(Bgra4444),
                typeof(Bgra5551),
                typeof(A8)
            };
        }

        public ColorPixelCollection(int width, int height) : this(new(width, height)) { }

        public ColorPixelCollection(int width, int height, TPixel pixel) : this(new(width, height, pixel)) { }

        public ColorPixelCollection(Image<TPixel> image)
        {
            ArgumentNullException.ThrowIfNull(image, nameof(image));

            _image = image;

            TransparentPixel = default;
            if (_transparentPixelTypes.Contains(typeof(TPixel)))
                SupportTransparent = true;
            else
                SupportTransparent = false;
        }

        private static readonly HashSet<Type> _transparentPixelTypes;

        private readonly Image<TPixel> _image;

        public TPixel this[int index] { get => this[index / Width, index % Width]; set => this[index / Width, index % Width] = value; }

        public TPixel this[int x, int y] { get => _image[x, y]; set => _image[x, y] = value; }

        public int Count => _image.Width * _image.Height;

        public int Width => _image.Width;

        public int Height => _image.Height;

        public SearchMode SearchMode => SearchMode.Coordinate;

        public virtual bool SupportTransparent { get; }

        public virtual TPixel TransparentPixel { get; }

        public OverwriteContext Overwrite(IPixelCollection<TPixel> pixels, Size size, Point location, Point offset)
        {
            ArgumentNullException.ThrowIfNull(pixels, nameof(pixels));

            OverwriteContext overwriteContext = new(new(Width, Height), new(pixels.Width, pixels.Height), new(size.Width, size.Height), location, offset);
            if (pixels.SupportTransparent)
            {
                TPixel transparent = pixels.TransparentPixel;
                foreach (var mapping in overwriteContext)
                {
                    TPixel pixel = pixels[mapping.OverwritePosition.X, mapping.OverwritePosition.Y];
                    if (!Equals(pixel, transparent))
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

        public void Fill(TPixel pixel)
        {
            _image.Mutate(ctx => ctx.Fill(Color.FromPixel(pixel)));
        }

        public IDictionary<Point, TPixel> GetAllPixel()
        {
            TPixel[] pixels = new TPixel[_image.Width * _image.Height];
            Span<TPixel> span = new(pixels);
            _image.CopyPixelDataTo(span);
            PositionEnumerable positions = new(_image.Width, _image.Height);

            Dictionary<Point, TPixel> result = new();
            Foreach.Start(positions, pixels, (position, pixel) => result.Add(position, pixel));

            return result;
        }

        public TPixel[] ToArray()
        {
            TPixel[] pixels = new TPixel[_image.Width * _image.Height];
            Span<TPixel> span = new(pixels);
            _image.CopyPixelDataTo(span);
            return pixels;
        }

        public void CopyPixelDataTo(Span<TPixel> destination)
        {
            _image.CopyPixelDataTo(destination);
        }

        protected override void DisposeUnmanaged()
        {
            _image.Dispose();
        }

        public ColorPixelCollection<TPixel> Clone()
        {
            return new(_image.Clone());
        }

        public IEnumerator<TPixel> GetEnumerator()
        {
            TPixel[] pixels = new TPixel[_image.Width * _image.Height];
            Span<TPixel> span = new(pixels);
            _image.CopyPixelDataTo(span);
            foreach (TPixel pixel in pixels)
            {
                yield return pixel;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
