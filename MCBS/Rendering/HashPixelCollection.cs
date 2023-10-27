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
    public class HashPixelCollection : IPixelCollection<int>
    {
        public HashPixelCollection(int width, int height, int pixel)
        {
            ThrowHelper.ArgumentOutOfMin(0, width, nameof(width));
            ThrowHelper.ArgumentOutOfMin(0, height, nameof(height));

            Width = width;
            Height = height;
            _hashs = new int[width * height];
            Fill(pixel);
        }

        private readonly int[] _hashs;

        public int this[int index] { get => _hashs[index]; set => _hashs[index] = value; }

        public int this[int x, int y] { get => _hashs[ToIndex(x, y)]; set => _hashs[ToIndex(x, y)] = value; }

        public int Count => _hashs.Length;

        public int Width { get; }

        public int Height { get; }

        public virtual bool SupportTransparent => true;

        public int TransparentPixel => string.Empty.GetHashCode();

        public void Fill(int pixel)
        {
            new Span<int>(_hashs).Fill(pixel);
        }

        public OverwriteContext Overwrite(IPixelCollection<int> pixels, Point location)
        {
            if (pixels is null)
                throw new ArgumentNullException(nameof(pixels));

            OverwriteContext overwriteContext = new(new(Width, Height), new(pixels.Width, pixels.Height), location);
            if (location == Point.Empty && pixels.Width == Width && pixels.Height == Height)
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

        public void CopyPixelDataTo(Span<int> destination)
        {
            new Span<int>(_hashs).CopyTo(destination);
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
