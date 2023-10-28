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

namespace MCBS.Rendering
{
    public class ColorPixelCollection<TPixel> : IPixelCollection<TPixel> where TPixel : unmanaged, IPixel<TPixel>
    {
        public ColorPixelCollection(int width, int height) : this(new(width, height)) { }

        public ColorPixelCollection(int width, int height, TPixel pixel) : this(new(width, height, pixel)) { }

        public ColorPixelCollection(Image<TPixel> image)
        {
            _image = image ?? throw new ArgumentNullException(nameof(image));

            TransparentPixel = default;
            Type type = typeof(TPixel);
            if (type == typeof(Rgba32) ||
                type == typeof(Rgba64) ||
                type == typeof(Rgba1010102) ||
                type == typeof(Argb32) ||
                type == typeof(Abgr32) ||
                type == typeof(Bgra32) ||
                type == typeof(Bgra4444) ||
                type == typeof(Bgra5551) ||
                type == typeof(A8))
            {
                SupportTransparent = true;
            }
        }

        private readonly Image<TPixel> _image;

        public TPixel this[int index] { get => this[index / Width, index % Width]; set => this[index / Width, index % Width] = value; }

        public TPixel this[int x, int y] { get => _image[x, y]; set => _image[x, y] = value; }

        public int Count => _image.Width * _image.Height;

        public int Width => _image.Width;

        public int Height => _image.Height;

        public SearchMode SearchMode => SearchMode.Coordinate;

        public virtual bool SupportTransparent { get; }

        public virtual TPixel TransparentPixel { get; }

        public OverwriteContext Overwrite(IPixelCollection<TPixel> pixels, Point location)
        {
            if (pixels is null)
                throw new ArgumentNullException(nameof(pixels));

            OverwriteContext overwriteContext = new(new(Width, Height), new(pixels.Width, pixels.Height), location);
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
