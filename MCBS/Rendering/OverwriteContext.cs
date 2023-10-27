using SixLabors.ImageSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Rendering
{
    public class OverwriteContext : IEnumerable<OverwriteMapping>
    {
        public OverwriteContext(Size baseSize, Size overwriteSize, Point location)
        {
            if (baseSize.Width < 0 || baseSize.Height < 0)
                throw new ArgumentException("宽度或长度不能小于0", nameof(baseSize));
            if (overwriteSize.Width < 0 || overwriteSize.Height < 0)
                throw new ArgumentException("宽度或长度不能小于0", nameof(overwriteSize));

            BaseSize = baseSize;
            OverwriteSize = overwriteSize;

            Point baseStartPosition = Point.Empty;
            Point baseEndPosition = new(overwriteSize.Width - 1, overwriteSize.Height - 1);
            Point overwriteStartPosition = Point.Empty;
            Point overwriteEndPosition = new(overwriteSize.Width - 1, overwriteSize.Height - 1);

            if (location.X < 0)
            {
                baseEndPosition.X += location.X;
                overwriteStartPosition.X -= location.X;
            }
            else
            {
                baseEndPosition.X += location.X;
                baseStartPosition.X += location.X;
            }

            if (location.Y < 0)
            {
                baseEndPosition.Y += location.Y;
                overwriteStartPosition.Y -= location.Y;
            }
            else
            {
                baseStartPosition.Y += location.Y;
                baseEndPosition.Y += location.Y;
            }

            Point maxEndPosition = new(baseSize.Width - 1, baseSize.Height - 1);
            Point overflow = new(baseEndPosition.X - maxEndPosition.X, baseEndPosition.Y - maxEndPosition.Y);

            if (overflow.X > 0)
            {
                baseEndPosition.X -= overflow.X;
                overwriteEndPosition.X -= overflow.X;
            }

            if (overflow.Y > 0)
            {
                baseEndPosition.Y -= overflow.Y;
                overwriteEndPosition.Y -= overflow.Y;
            }

            BaseStartPosition = baseStartPosition;
            BaseEndPosition = baseEndPosition;
            OverwriteStartPosition = overwriteStartPosition;
            OverwriteEndPosition = overwriteEndPosition;
            OverwriteRectangle = new(baseStartPosition.X, baseStartPosition.Y, baseEndPosition.X - baseStartPosition.X + 1, baseEndPosition.Y - baseStartPosition.Y + 1);
        }

        public Size BaseSize { get; }

        public Size OverwriteSize { get; }

        public Point BaseStartPosition { get; }

        public Point BaseEndPosition { get; }

        public Point OverwriteStartPosition { get; }

        public Point OverwriteEndPosition { get; }

        public Rectangle OverwriteRectangle { get; }

        public int OverwritePixelCount => OverwriteRectangle.Size.Width * OverwriteRectangle.Size.Height;

        public IEnumerator<OverwriteMapping> GetEnumerator()
        {
            Point indexPosition = Point.Empty;
            Point basePosition = BaseStartPosition;
            Point overwritePosition = OverwriteStartPosition;

            for (int y = BaseStartPosition.Y; y <= BaseEndPosition.Y; y++)
            {
                for (int x = BaseStartPosition.X; x <= BaseEndPosition.X; x++)
                {
                    yield return new(indexPosition, basePosition, overwritePosition);
                    indexPosition.X++;
                    basePosition.X++;
                    overwritePosition.X++;
                }
                indexPosition.X = 0;
                basePosition.X = BaseStartPosition.X;
                overwritePosition.X = OverwriteStartPosition.X;
                indexPosition.Y++;
                basePosition.Y++;
                overwritePosition.Y++;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
