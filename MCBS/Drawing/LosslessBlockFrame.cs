using QuanLib.Core;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Drawing
{
    public class LosslessBlockFrame : BlockFrame
    {
        public LosslessBlockFrame(int width, int height, string? pixel = null)
        {
            ThrowHelper.ArgumentOutOfMin(1, width, nameof(width));
            ThrowHelper.ArgumentOutOfMin(1, height, nameof(height));

            Width = width;
            Height = height;
            _blocks = new string[width * height];
            ContainsTransparent = string.IsNullOrEmpty(pixel);

            if (!string.IsNullOrEmpty(pixel))
                Fill(pixel);
        }

        private LosslessBlockFrame(int width, int height, string[] pixels)
        {
            ThrowHelper.ArgumentOutOfMin(1, width, nameof(width));
            ThrowHelper.ArgumentOutOfMin(1, height, nameof(height));
            ArgumentNullException.ThrowIfNull(pixels, nameof(pixels));
            ThrowHelper.ArrayLengthOutOfRange(width * height, pixels, nameof(pixels));

            Width = width;
            Height = height;
            _blocks = pixels;
        }

        public override string this[int index] { get => _blocks[index]; set => _blocks[index] = value; }

        public override string this[int x, int y] { get => _blocks[ToIndex(x, y)]; set => _blocks[ToIndex(x, y)] = value; }

        private readonly string[] _blocks;

        public override int Count => Width * Height;

        public override int Width { get; }

        public override int Height { get; }

        public override SearchMode SearchMode => SearchMode.Index;

        public override bool IsTransparentPixel(int index)
        {
            return string.IsNullOrEmpty(this[index]);
        }

        public override bool IsTransparentPixel(int x, int y)
        {
            return string.IsNullOrEmpty(this[x, y]);
        }

        public override bool CheckTransparentPixel()
        {
            foreach (string block in _blocks)
            {
                if (string.IsNullOrEmpty(block))
                    return true;
            }

            return false;
        }

        public override BlockFrame Crop(Rectangle rectangle)
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

            string[] array = new string[count];
            int index1 = rectangle.Y * Width + rectangle.X;
            int index2 = 0;

            if (rectangle.Width == Width)
            {
                Array.Copy(_blocks, index1, array, index2, count);
            }
            else
            {
                for (int i = 0; i < rectangle.Height; i++)
                {
                    Array.Copy(_blocks, index1, array, index2, rectangle.Width);
                    index1 += Width;
                    index2 += rectangle.Width;
                }
            }

            return new LosslessBlockFrame(rectangle.Width, rectangle.Height, array);
        }

        public override BlockFrame Clone()
        {
            return new LosslessBlockFrame(Width, Height, ToArray());
        }

        public override void Fill(string pixel)
        {
            new Span<string>(_blocks).Fill(pixel);
        }

        public override string[] ToArray()
        {
            string[] array = new string[_blocks.Length];
            new Span<string>(_blocks).CopyTo(new(array));
            return array;
        }

        private int ToIndex(int x, int y)
        {
            return y * Width + x;
        }

        public override ScreenPixel<string>[] GetAllPixel()
        {
            PositionEnumerable positions = new(Width, Height);
            ScreenPixel<string>[] result = new ScreenPixel<string>[Count];
            int index = 0;

            Foreach.Start(positions, _blocks, (position, pixel) => result[index++] = new(position, pixel));

            return result;
        }

        public override IEnumerator<string> GetEnumerator()
        {
            foreach (string block in _blocks)
            {
                yield return block;
            }
        }
    }
}
