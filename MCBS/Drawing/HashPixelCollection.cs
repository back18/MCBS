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
    public class HashPixelCollection : IPixelCollection<int>
    {
        public HashPixelCollection(int width, int height, int pixel)
        {
            ThrowHelper.ArgumentOutOfMin(1, width, nameof(width));
            ThrowHelper.ArgumentOutOfMin(1, height, nameof(height));

            Width = width;
            Height = height;
            TransparentPixel = string.Empty.GetHashCode();
            _hashs = new int[width * height];

            if (pixel != 0)
                Fill(pixel);
        }

        private HashPixelCollection(int width, int height, int[] pixels)
        {
            ThrowHelper.ArgumentOutOfMin(1, width, nameof(width));
            ThrowHelper.ArgumentOutOfMin(1, height, nameof(height));
            ArgumentNullException.ThrowIfNull(pixels, nameof(pixels));
            ThrowHelper.ArrayLengthOutOfRange(width * height, pixels, nameof(pixels));

            Width = width;
            Height = height;
            TransparentPixel = string.Empty.GetHashCode();
            _hashs = pixels;
        }

        public int this[int index] { get => _hashs[index]; set => _hashs[index] = value; }

        public int this[int x, int y] { get => _hashs[ToIndex(x, y)]; set => _hashs[ToIndex(x, y)] = value; }

        private readonly int[] _hashs;

        public int Count => _hashs.Length;

        public int Width { get; }

        public int Height { get; }

        public SearchMode SearchMode => SearchMode.Index;

        public virtual bool SupportTransparent => true;

        public int TransparentPixel { get; }

        public bool IsTransparentPixel(int index)
        {
            return this[index] == TransparentPixel;
        }

        public bool IsTransparentPixel(int x, int y)
        {
            return this[x, y] == TransparentPixel;
        }

        public bool CheckTransparentPixel()
        {
            foreach (int hash in _hashs)
            {
                if (hash == TransparentPixel)
                    return true;
            }

            return false;
        }

        public OverwriteContext Overwrite(IPixelCollection<int> pixels, Size size, Point location, Point offset)
        {
            ArgumentNullException.ThrowIfNull(pixels, nameof(pixels));

            OverwriteContext overwriteContext = new(new(Width, Height), new(pixels.Width, pixels.Height), new(size.Width, size.Height), location, offset);
            if (location == Point.Empty && offset == Point.Empty && pixels.Width == Width && pixels.Height == Height)
            {
                if (pixels.SupportTransparent)
                {
                    int transparent = pixels.TransparentPixel;
                    for (int i = 0; i < _hashs.Length; i++)
                    {
                        int pixel = pixels[i];
                        if (pixel != transparent)
                            _hashs[i] = pixel;
                    }
                }
                else
                {
                    Span<int> span = new(_hashs);
                    pixels.CopyPixelDataTo(span);
                }
            }
            else
            {
                if (pixels.SupportTransparent)
                {
                    int transparent = pixels.TransparentPixel;
                    foreach (var mapping in overwriteContext)
                    {
                        int pixel = pixels[mapping.OverwritePosition.X, mapping.OverwritePosition.Y];
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
            }

            return overwriteContext;
        }

        public void Fill(int pixel)
        {
            new Span<int>(_hashs).Fill(pixel);
        }

        public ScreenPixel<int>[] GetAllPixel()
        {
            PositionEnumerable positions = new(Width, Height);
            ScreenPixel<int>[] result = new ScreenPixel<int>[Count];
            int index = 0;

            Foreach.Start(positions, _hashs, (position, pixel) => result[index++] = new(position, pixel));

            return result;
        }

        public int[] ToArray()
        {
            int[] array = new int[_hashs.Length];
            new Span<int>(_hashs).CopyTo(new(array));
            return array;
        }

        public void CopyPixelDataTo(Span<int> destination)
        {
            new Span<int>(_hashs).CopyTo(destination);
        }

        public HashPixelCollection Crop(Rectangle rectangle)
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

            int[] array = new int[count];
            int index1 = rectangle.Y * Width + rectangle.X;
            int index2 = 0;

            if (rectangle.Width == Width)
            {
                Array.Copy(_hashs, index1, array, index2, count);
            }
            else
            {
                for (int i = 0; i < rectangle.Height; i++)
                {
                    Array.Copy(_hashs, index1, array, index2, rectangle.Width);
                    index1 += Width;
                    index2 += rectangle.Width;
                }
            }

            return new(rectangle.Width, rectangle.Height, array);
        }

        public HashPixelCollection Clone()
        {
            return new(Width, Height, ToArray());
        }

        private int ToIndex(int x, int y)
        {
            return y * Width + x;
        }

        public IEnumerator<int> GetEnumerator()
        {
            foreach (int hash in _hashs)
            {
                yield return hash;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
