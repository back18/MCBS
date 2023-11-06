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
        public OverwriteContext(Size baseSize, Point baseLocation, Size overwriteSize, Point overwriteLocation)
        {
            if (baseSize.Width < 0 || baseSize.Height < 0)
                throw new ArgumentException("宽度或长度不能小于0", nameof(baseSize));
            if (overwriteSize.Width < 0 || overwriteSize.Height < 0)
                throw new ArgumentException("宽度或长度不能小于0", nameof(overwriteSize));

            BaseSize = baseSize;
            OverwriteSize = overwriteSize;

            Point baseStartPosition = Point.Empty;
            Point baseEndPosition = new(overwriteSize.Width - 1, overwriteSize.Height - 1);
            Point overwriteStartPosition = overwriteLocation;
            Point overwriteEndPosition = new(overwriteSize.Width - 1, overwriteSize.Height - 1);

            if (baseLocation.X < 0)
            {
                baseEndPosition.X += baseLocation.X;
                overwriteStartPosition.X -= baseLocation.X;
            }
            else
            {
                baseEndPosition.X += baseLocation.X;
                baseStartPosition.X += baseLocation.X;
            }

            if (baseLocation.Y < 0)
            {
                baseEndPosition.Y += baseLocation.Y;
                overwriteStartPosition.Y -= baseLocation.Y;
            }
            else
            {
                baseStartPosition.Y += baseLocation.Y;
                baseEndPosition.Y += baseLocation.Y;
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
