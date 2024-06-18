using SixLabors.ImageSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Drawing
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

            if (baseStartPosition.X < 0)
            {
                overwriteStartPosition.X -= baseStartPosition.X;
                cropSize.Width += baseStartPosition.X;
                baseStartPosition.X = 0;
            }

            if (baseStartPosition.Y < 0)
            {
                overwriteStartPosition.Y -= baseStartPosition.Y;
                cropSize.Height += baseStartPosition.Y;
                baseStartPosition.Y = 0;
            }

            if (overwriteStartPosition.X < 0)
            {
                cropSize.Width += overwriteStartPosition.X;
                overwriteStartPosition.X = 0;
            }

            if (overwriteStartPosition.Y < 0)
            {
                cropSize.Height += overwriteStartPosition.Y;
                overwriteStartPosition.Y = 0;
            }

            Point maxBaseEndPosition = new(baseSize.Width - 1, baseSize.Height - 1);
            Point baseOverflow = new(baseEndPosition.X - maxBaseEndPosition.X, baseEndPosition.Y - maxBaseEndPosition.Y);

            if (baseOverflow.X > 0)
            {
                cropSize.Width -= baseOverflow.X;
                baseEndPosition.X -= baseOverflow.X;
                overwriteEndPosition.X -= baseOverflow.X;
            }

            if (baseOverflow.Y > 0)
            {
                cropSize.Height -= baseOverflow.Y;
                baseEndPosition.Y -= baseOverflow.Y;
                overwriteEndPosition.Y -= baseOverflow.Y;
            }

            Point maxOverwriteEndPosition = new(overwriteSize.Width - 1, overwriteSize.Height - 1);
            Point overwriteOverflow = new(overwriteEndPosition.X - maxOverwriteEndPosition.X, overwriteEndPosition.Y - maxOverwriteEndPosition.Y);

            if (overwriteOverflow.X > 0)
            {
                cropSize.Width -= overwriteOverflow.X;
                baseEndPosition.X -= overwriteOverflow.X;
                overwriteEndPosition.X -= overwriteOverflow.X;
            }

            if (overwriteOverflow.Y > 0)
            {
                cropSize.Height -= overwriteOverflow.Y;
                baseEndPosition.Y -= overwriteOverflow.Y;
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
