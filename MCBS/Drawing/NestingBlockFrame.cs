using QuanLib.Core;
using QuanLib.Core.Extensions;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Drawing
{
    public class NestingBlockFrame : BlockFrame
    {
        public NestingBlockFrame(
            int width,
            int height,
            BlockFrame innerBlockFrame,
            Point location,
            Point offset,
            int borderWidth,
            string backgroundColor,
            string borderColor)
        {
            ThrowHelper.ArgumentOutOfMin(1, width, nameof(width));
            ThrowHelper.ArgumentOutOfMin(1, height, nameof(height));
            ArgumentNullException.ThrowIfNull(innerBlockFrame, nameof(innerBlockFrame));
            ArgumentNullException.ThrowIfNull(backgroundColor, nameof(backgroundColor));
            ArgumentNullException.ThrowIfNull(borderColor, nameof(borderColor));

            Width = width;
            Height = height;
            InnerBlockFrame = innerBlockFrame;
            Location = location;
            Offset = offset;
            BorderWidth = borderWidth;
            BackgroundColor = backgroundColor;
            BorderColor = borderColor;

            _isTransparentBackground = string.IsNullOrEmpty(backgroundColor);
            _isTransparentBorder = string.IsNullOrEmpty(borderColor);
            _innerPosition = new(new(width, height), new(innerBlockFrame.Width, innerBlockFrame.Height), location, offset, borderWidth);
            _xPixelTypes = new PixelType[Width];
            _yPixelTypes = new PixelType[Height];
            _totalOffset = new(location.X - offset.X, location.Y - offset.Y);

            if (borderWidth > 0)
            {
                Size borderSize = new(
                _innerPosition.BorderEndPosition.X - _innerPosition.BorderStartPosition.X + 1,
                _innerPosition.BorderEndPosition.Y - _innerPosition.BorderStartPosition.Y + 1);
                if (borderSize.Width > 0)
                    Array.Fill(_xPixelTypes, PixelType.Border, _innerPosition.BorderStartPosition.X, borderSize.Width);
                if (borderSize.Height > 0)
                    Array.Fill(_yPixelTypes, PixelType.Border, _innerPosition.BorderStartPosition.Y, borderSize.Height);
            }

            Size innerSize = new(
                _innerPosition.BlockFrameEndPosition.X - _innerPosition.BlockFrameStartPosition.X + 1,
                _innerPosition.BlockFrameEndPosition.Y - _innerPosition.BlockFrameStartPosition.Y + 1);
            if (innerSize.Width > 0)
                Array.Fill(_xPixelTypes, PixelType.Inner, _innerPosition.BlockFrameStartPosition.X, innerSize.Width);
            if (innerSize.Height > 0)
                Array.Fill(_yPixelTypes, PixelType.Inner, _innerPosition.BlockFrameStartPosition.Y, innerSize.Height);
        }

        public override string this[int index] { get => this[index / Width, index % Width]; set => throw new NotSupportedException(); }

        public override string this[int x, int y]
        {
            get
            {
                PixelType xPixelType = _xPixelTypes[x];
                PixelType yPixelType = _yPixelTypes[y];

                if (xPixelType == PixelType.Background || yPixelType == PixelType.Background)
                    return BackgroundColor;

                if (xPixelType == PixelType.Border || yPixelType == PixelType.Border)
                    return BorderColor;

                x -= _totalOffset.X;
                y -= _totalOffset.Y;

                if (InnerBlockFrame.IsTransparentPixel(x, y))
                    return BackgroundColor;
                else
                    return InnerBlockFrame[x, y];
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        private readonly bool _isTransparentBackground;

        private readonly bool _isTransparentBorder;

        private readonly InnerPosition _innerPosition;

        private readonly PixelType[] _xPixelTypes;

        private readonly PixelType[] _yPixelTypes;

        private readonly Point _totalOffset;

        public BlockFrame InnerBlockFrame { get; }

        public InnerPosition InnerPosition => _innerPosition;

        public Point Location { get; }

        public Point Offset { get; }

        public int BorderWidth { get; }

        public string BackgroundColor { get; }

        public string BorderColor { get; }

        public override int Count => Width * Height;

        public override int Width { get; }

        public override int Height { get; }

        public override SearchMode SearchMode => SearchMode.Coordinate;

        public override bool IsTransparentPixel(int index)
        {
            return IsTransparentPixel(index / Width, index % Width);
        }

        public override bool IsTransparentPixel(int x, int y)
        {
            PixelType xPixelType = _xPixelTypes[x];
            PixelType yPixelType = _yPixelTypes[y];

            if (xPixelType == PixelType.Background || yPixelType == PixelType.Background)
                return _isTransparentBackground;

            if (xPixelType == PixelType.Border || yPixelType == PixelType.Border)
                return _isTransparentBorder;

            if (InnerBlockFrame.IsTransparentPixel(x - _totalOffset.X, y - _totalOffset.Y))
                return _isTransparentBackground;
            else
                return false;
        }

        public override bool CheckTransparentPixel()
        {
            if (!_isTransparentBackground)
                return false;

            if (_innerPosition.BorderStartPosition.IsEmpty &&
                _innerPosition.BorderEndPosition == new Point(Width- 1, Height-1))
            {
                if (BorderWidth > 0 && _isTransparentBorder)
                    return true;
                else
                    return InnerBlockFrame.CheckTransparentPixel();
            }
            else
            {
                return true;
            }
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

            if (rectangle.Right < _innerPosition.BorderStartPosition.X ||
                rectangle.Bottom < _innerPosition.BorderStartPosition.Y ||
                rectangle.Left > _innerPosition.BorderEndPosition.X ||
                rectangle.Top > _innerPosition.BorderEndPosition.Y)
                return new HashBlockFrame(rectangle.Width, rectangle.Height, BackgroundColor);

            if (!InnerBlockFrame.SupportTransparent &&
                rectangle.Left >= _innerPosition.BorderStartPosition.X &&
                rectangle.Top >= _innerPosition.BlockFrameStartPosition.Y &&
                rectangle.Right <= _innerPosition.BlockFrameEndPosition.X &&
                rectangle.Bottom <= _innerPosition.BlockFrameEndPosition.Y)
            {
                rectangle.X += Location.X;
                rectangle.Y += Location.Y;
                return InnerBlockFrame.Crop(rectangle);
            }

            return base.Crop(rectangle);
        }

        public override BlockFrame Clone()
        {
            return new NestingBlockFrame(Width, Height, InnerBlockFrame, Location, Offset, BorderWidth, BackgroundColor, BorderColor);
        }

        public override void Fill(string pixel)
        {
            throw new NotSupportedException();
        }

        public override ScreenPixel<string>[] GetAllPixel()
        {
            PositionEnumerable positions = new(Width, Height);
            ScreenPixel<string>[] result = new ScreenPixel<string>[Count];
            int index = 0;

            Foreach.Start(positions, this, (position, pixel) => result[index++] = new(position, pixel));

            return result;
        }

        public override string[] ToArray()
        {
            string[] result = new string[Count];
            int y1 = Math.Clamp(_innerPosition.BorderStartPosition.Y, 0, Height);
            int y2 = Math.Clamp(_innerPosition.BorderEndPosition.Y + 1, 0, Height);
            int h1 = y1;
            int h2 = Height - y2;
            int index = h1 * Width;

            if (h1 > 0)
                result.Fill(BackgroundColor, 0, index);

            for (int y = y1; y < y2; y++)
                for (int x = 0; x < Width; x++)
                    result[index++] = this[x, y];

            if (h2 > 0)
                result.Fill(BackgroundColor, index, result.Length - index);

            return result;
        }

        public override IEnumerator<string> GetEnumerator()
        {
            int y1 = Math.Clamp(_innerPosition.BorderStartPosition.Y, 0, Height);
            int y2 = Math.Clamp(_innerPosition.BorderEndPosition.Y + 1, 0, Height);

            for (int y = 0; y < y1; y++)
                for (int x = 0; x < Width; x++)
                    yield return BackgroundColor;

            Point position = new(-_totalOffset.X, y1 - _totalOffset.Y);
            int x1 = Math.Clamp(_innerPosition.BlockFrameStartPosition.X, 0, Width);
            int x2 = Math.Clamp(_innerPosition.BlockFrameEndPosition.X + 1, 0, Width);

            for (int y = y1; y < y2; y++, position.Y++)
            {
                if (position.Y < _innerPosition.BlockFrameStartPosition.Y ||
                    position.Y > _innerPosition.BlockFrameEndPosition.Y)
                {
                    for (int x = 0; x < Width; x++)
                        yield return this[x, y];
                }
                else
                {
                    for (int x = 0; x < x1; x++)
                        yield return this[x, y];

                    position.X = x1 -_totalOffset.X;
                    for (int x = x1; x < x2; x++, position.X++)
                    {
                        if (InnerBlockFrame.IsTransparentPixel(position.X, position.Y))
                            yield return BackgroundColor;
                        else
                            yield return InnerBlockFrame[position.X, position.Y];
                    }

                    for (int x = x2; x < Width; x++)
                        yield return this[x, y];
                }
            }

            for (int y = y2; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    yield return BackgroundColor;
        }

        private enum PixelType
        {
            Background,

            Border,

            Inner
        }
    }
}
