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
        public OverwriteContext(Size baseSize, Size overwriteSize, Size cropSize, Point baseLocation, Point overwriteLocation)
        {
            if (baseSize.Width < 0 || baseSize.Height < 0)
                throw new ArgumentException("宽度或长度不能小于0", nameof(baseSize));
            if (overwriteSize.Width < 0 || overwriteSize.Height < 0)
                throw new ArgumentException("宽度或长度不能小于0", nameof(overwriteSize));

            BaseSize = baseSize;
            OverwriteSize = overwriteSize;
            CropSize = cropSize;

            Point baseStartPosition = baseLocation;
            Point baseEndPosition = new(baseLocation.X + cropSize.Width - 1, baseLocation.Y + cropSize.Height - 1);
            Point overwriteStartPosition = overwriteLocation;
            Point overwriteEndPosition = new(overwriteLocation.X + cropSize.Width - 1, overwriteLocation.Y + cropSize.Height - 1);

            if (baseLocation.X < 0)
            {
                baseEndPosition.X += baseLocation.X;
                overwriteStartPosition.X -= baseLocation.X;
            }

            if (baseLocation.Y < 0)
            {
                baseEndPosition.Y += baseLocation.Y;
                overwriteStartPosition.Y -= baseLocation.Y;
            }

            if (overwriteLocation.X < 0)
            {
                overwriteStartPosition.X += overwriteLocation.X;
                overwriteEndPosition.X += overwriteLocation.X;
            }

            if (overwriteLocation.Y < 0)
            {
                overwriteStartPosition.Y += overwriteLocation.Y;
                overwriteEndPosition.Y += overwriteLocation.Y;
            }

            Point maxBaseEndPosition = new(baseSize.Width - 1, baseSize.Height - 1);
            Point baseOverflow = new(baseEndPosition.X - maxBaseEndPosition.X, baseEndPosition.Y - maxBaseEndPosition.Y);

            if (baseOverflow.X > 0)
            {
                baseEndPosition.X -= baseOverflow.X;
                overwriteEndPosition.X -= baseOverflow.X;
            }

            if (baseOverflow.Y > 0)
            {
                baseEndPosition.Y -= baseOverflow.Y;
                overwriteEndPosition.Y -= baseOverflow.Y;
            }

            Point maxOverwriteEndPosition = new(overwriteSize.Width - 1, overwriteSize.Height - 1);
            Point overwriteOverflow = new(overwriteEndPosition.X - maxOverwriteEndPosition.X, overwriteEndPosition.Y - maxOverwriteEndPosition.Y);

            if (overwriteOverflow.X > 0)
            {
                baseEndPosition.Y -= overwriteOverflow.X;
                overwriteStartPosition.X -= overwriteOverflow.X;
                overwriteEndPosition.Y -= overwriteOverflow.Y;
            }

            if (overwriteOverflow.Y > 0)
            {
                baseEndPosition.X -= overwriteOverflow.X;
                overwriteStartPosition.Y -= overwriteOverflow.Y;
                overwriteEndPosition.Y -= overwriteOverflow.Y;
            }

            BaseStartPosition = baseStartPosition;
            BaseEndPosition = baseEndPosition;
            OverwriteStartPosition = overwriteStartPosition;
            OverwriteEndPosition = overwriteEndPosition;
            OverwriteRectangle = new(baseStartPosition.X, baseStartPosition.Y, baseEndPosition.X - baseStartPosition.X + 1, baseEndPosition.Y - baseStartPosition.Y + 1);
        }

        public Size BaseSize { get; }

        public Size OverwriteSize { get; }

        public Size CropSize { get; }

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
