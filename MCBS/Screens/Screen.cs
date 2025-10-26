using QuanLib.Core;
using QuanLib.DataAnnotations;
using QuanLib.Game;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Screens
{
    /// <summary>
    /// 屏幕
    /// </summary>
    public struct Screen : IPlane, IEquatable<Screen>, IDataModelOwner<Screen, Screen.DataModel>
    {
        public Screen(DataModel model)
        {
            NullValidator.ValidateObject(model, nameof(model));
            ThrowHelper.ArgumentOutOfRange(1, 512, model.Width, "model.Width");
            ThrowHelper.ArgumentOutOfRange(1, 512, model.Height, "model.Height");

            StartPosition = new(model.StartPosition[0], model.StartPosition[1], model.StartPosition[2]);
            Width = model.Width;
            Height = model.Height;
            XFacing = (Facing)model.XFacing;
            YFacing = (Facing)model.YFacing;
        }

        public Screen(Vector3<int> startPosition, int width, int height, Facing xFacing, Facing yFacing)
        {
            ThrowHelper.ArgumentOutOfRange(1, 512, width, nameof(width));
            ThrowHelper.ArgumentOutOfRange(1, 512, height, nameof(height));

            StartPosition = startPosition;
            Width = width;
            Height = height;
            XFacing = xFacing;
            YFacing = yFacing;
        }

        public Vector3<int> StartPosition { get; private set; }

        public readonly Vector3<int> EndPosition => ScreenPos2WorldPos(new(Width - 1, Height - 1));

        public readonly Vector3<int> CenterPosition => ScreenPos2WorldPos(new(Width / 2, Height / 2));

        public int Width { get; private set; }

        public int Height { get; private set; }

        public Facing XFacing { get; private set; }

        public Facing YFacing { get; private set; }

        public readonly Facing NormalFacing => new PlaneFacing(XFacing, YFacing).NormalFacing;

        public readonly PlaneAxis PlaneAxis
        {
            get
            {
                return NormalFacing switch
                {
                    Facing.Xp or Facing.Xm => PlaneAxis.ZY,
                    Facing.Yp or Facing.Ym => PlaneAxis.XZ,
                    Facing.Zp or Facing.Zm => PlaneAxis.XY,
                    _ => throw new InvalidOperationException(),
                };
            }
        }

        public readonly int PlaneCoordinate
        {
            get
            {
                return PlaneAxis switch
                {
                    PlaneAxis.XY => StartPosition.Z,
                    PlaneAxis.ZY => StartPosition.X,
                    PlaneAxis.XZ => StartPosition.Y,
                    _ => throw new InvalidOperationException(),
                };
            }
        }

        public readonly int TotalPixels => Width * Height;

        public Screen SetPosition(Vector3<int> position)
        {
            StartPosition = position;
            return this;
        }

        public Screen SetPosition(int x, int y, int z) => SetPosition(new(x, y, z));

        public Screen SetSize(int width, int height)
        {
            ThrowHelper.ArgumentOutOfMin(1, width, nameof(width));
            ThrowHelper.ArgumentOutOfMin(1, height, nameof(height));

            Width = width;
            Height = height;
            return this;
        }

        public Screen SetSize(Size size) => SetSize(size.Width, size.Height);

        public Screen UpRotate()
        {
            return ApplyRotate(new PlaneFacing(XFacing, YFacing).UpRotate());
        }

        public Screen DownRotate()
        {
            return ApplyRotate(new PlaneFacing(XFacing, YFacing).DownRotate());
        }

        public Screen LeftRotate()
        {
            return ApplyRotate(new PlaneFacing(XFacing, YFacing).LeftRotate());
        }

        public Screen RightRotate()
        {
            return ApplyRotate(new PlaneFacing(XFacing, YFacing).RightRotate());
        }

        public Screen ClockwiseRotate()
        {
            return ApplyRotate(new PlaneFacing(XFacing, YFacing).ClockwiseRotate());
        }

        public Screen CounterclockwiseRotate()
        {
            return ApplyRotate(new PlaneFacing(XFacing, YFacing).CounterclockwiseRotate());
        }

        private Screen ApplyRotate(PlaneFacing planeFacing)
        {
            if (planeFacing.XFacing == XFacing && planeFacing.YFacing == YFacing)
                return this;

            Vector3<int> oldPos = CenterPosition;
            XFacing = planeFacing.XFacing;
            YFacing = planeFacing.YFacing;
            Vector3<int> newPos = CenterPosition;

            Vector3<int> offset = newPos - oldPos;
            StartPosition -= offset;

            return this;
        }

        public Screen ApplyTranslate(int dx, int dy)
        {
            Vector3<int> position = StartPosition;
            position = position.Offset(XFacing, dx);
            position = position.Offset(YFacing, dy);
            StartPosition = position;
            return this;
        }

        public Screen ApplyTranslate(Point offset) => ApplyTranslate(offset.X, offset.Y);

        public Screen ApplyAdvance(int offset)
        {
            StartPosition = StartPosition.Offset(NormalFacing, offset);
            return this;
        }

        public readonly CubeRange GetRange(int maxBackLayers = 0, int maxFrontLayers = 0)
        {
            ThrowHelper.ArgumentOutOfMin(0, maxBackLayers, nameof(maxBackLayers));
            ThrowHelper.ArgumentOutOfMin(0, maxFrontLayers, nameof(maxFrontLayers));

            Vector3<int> startPos = ScreenPos2WorldPos(Point.Empty);
            Vector3<int> endPos = ScreenPos2WorldPos(new(Width - 1, Height - 1));

            if (maxBackLayers == 0 && maxFrontLayers == 0)
                return new(startPos, endPos);

            int index = Math.Abs((int)NormalFacing) - 1;
            switch (NormalFacing)
            {
                case Facing.Xp:
                case Facing.Yp:
                case Facing.Zp:
                    endPos[index] += maxFrontLayers;
                    startPos[index] -= maxBackLayers;
                    break;
                case Facing.Xm:
                case Facing.Ym:
                case Facing.Zm:
                    startPos[index] -= maxFrontLayers;
                    endPos[index] += maxBackLayers;
                    break;
                default:
                    throw new InvalidOperationException();
            }

            return new(startPos, endPos);
        }

        public readonly bool InAltitudeRange(int min, int max)
        {
            Vector3<int> position1 = ScreenPos2WorldPos(new(0, 0));
            Vector3<int> position2 = ScreenPos2WorldPos(new(Width - 1, 0));
            Vector3<int> position3 = ScreenPos2WorldPos(new(0, Height - 1));
            Vector3<int> position4 = ScreenPos2WorldPos(new(Width - 1, Height - 1));

            return
                CheckHelper.Range(min, max, position1.Y) &&
                CheckHelper.Range(min, max, position2.Y) &&
                CheckHelper.Range(min, max, position3.Y) &&
                CheckHelper.Range(min, max, position4.Y);
        }

        public readonly Screen SubScreen(Rectangle rectangle) => SubScreen(rectangle.Location, rectangle.Size);

        public readonly Screen SubScreen(Point startPosition, Size size) => SubScreen(startPosition, size.Width, size.Height);

        public readonly Screen SubScreen(Point startPosition, int width, int height)
        {
            return new(ScreenPos2WorldPos(startPosition), width, height, XFacing, YFacing);
        }

        public readonly Vector3<int> ScreenPos2WorldPos(Point position, int layer = 0)
        {
            int x = 0;
            int y = 0;
            int z = 0;

            switch (XFacing)
            {
                case Facing.Xp:
                    x = StartPosition.X + position.X;
                    break;
                case Facing.Xm:
                    x = StartPosition.X - position.X;
                    break;
                case Facing.Yp:
                    y = StartPosition.Y + position.X;
                    break;
                case Facing.Ym:
                    y = StartPosition.Y - position.X;
                    break;
                case Facing.Zp:
                    z = StartPosition.Z + position.X;
                    break;
                case Facing.Zm:
                    z = StartPosition.Z - position.X;
                    break;
                default:
                    throw new InvalidOperationException();
            }

            switch (YFacing)
            {
                case Facing.Xp:
                    x = StartPosition.X + position.Y;
                    break;
                case Facing.Xm:
                    x = StartPosition.X - position.Y;
                    break;
                case Facing.Yp:
                    y = StartPosition.Y + position.Y;
                    break;
                case Facing.Ym:
                    y = StartPosition.Y - position.Y;
                    break;
                case Facing.Zp:
                    z = StartPosition.Z + position.Y;
                    break;
                case Facing.Zm:
                    z = StartPosition.Z - position.Y;
                    break;
                default:
                    throw new InvalidOperationException();
            }

            switch (NormalFacing)
            {
                case Facing.Xp:
                    x = StartPosition.X + layer;
                    break;
                case Facing.Xm:
                    x = StartPosition.X - layer;
                    break;
                case Facing.Yp:
                    y = StartPosition.Y + layer;
                    break;
                case Facing.Ym:
                    y = StartPosition.Y - layer;
                    break;
                case Facing.Zp:
                    z = StartPosition.Z + layer;
                    break;
                case Facing.Zm:
                    z = StartPosition.Z - layer;
                    break;
                default:
                    break;
            }

            return new(x, y, z);
        }

        public readonly Point WorldPos2ScreenPos(Vector3<int> position)
        {
            var x = XFacing switch
            {
                Facing.Xp => position.X - StartPosition.X,
                Facing.Xm => StartPosition.X - position.X,
                Facing.Yp => position.Y - StartPosition.Y,
                Facing.Ym => StartPosition.Y - position.Y,
                Facing.Zp => position.Z - StartPosition.Z,
                Facing.Zm => StartPosition.Z - position.Z,
                _ => throw new InvalidOperationException()
            };
            var y = YFacing switch
            {
                Facing.Xp => position.X - StartPosition.X,
                Facing.Xm => StartPosition.X - position.X,
                Facing.Yp => position.Y - StartPosition.Y,
                Facing.Ym => StartPosition.Y - position.Y,
                Facing.Zp => position.Z - StartPosition.Z,
                Facing.Zm => StartPosition.Z - position.Z,
                _ => throw new InvalidOperationException()
            };

            return new(x, y);
        }

        public readonly double GetPlaneDistance<T>(T position) where T : IVector3<double>
        {
            ArgumentNullException.ThrowIfNull(position, nameof(position));

            return NormalFacing switch
            {
                Facing.Xp => position.X - PlaneCoordinate,
                Facing.Xm => PlaneCoordinate - position.X,
                Facing.Yp => position.Y - PlaneCoordinate,
                Facing.Ym => PlaneCoordinate - position.Y,
                Facing.Zp => position.Z - PlaneCoordinate,
                Facing.Zm => PlaneCoordinate - position.Z,
                _ => throw new InvalidOperationException(),
            };
        }

        public readonly bool IncludedOnScreen(Point position)
        {
            return position.X >= 0 && position.Y >= 0 && position.X < Width && position.Y < Height;
        }

        public readonly bool IncludedOnScreen(Vector3<int> position)
        {
            bool isScreenPlane = NormalFacing switch
            {
                Facing.Xp or Facing.Xm => position.X == PlaneCoordinate,
                Facing.Yp or Facing.Ym => position.Y == PlaneCoordinate,
                Facing.Zp or Facing.Zm => position.Z == PlaneCoordinate,
                _ => throw new InvalidOperationException()
            };
            return isScreenPlane && IncludedOnScreen(WorldPos2ScreenPos(position));
        }

        public readonly bool Equals(Screen other)
        {
            return this == other;
        }

        public override readonly bool Equals([NotNullWhen(true)] object? obj)
        {
            return obj is Screen other && Equals(other);
        }

        public readonly override int GetHashCode()
        {
            return HashCode.Combine(StartPosition.X, StartPosition.Y, StartPosition.Z, Width, Height, XFacing, YFacing);
        }

        public override readonly string ToString()
        {
            return $"StartPosition={StartPosition}, Width={Width}, Height={Height}, XFacing={XFacing}, YFacing={YFacing}";
        }

        public readonly DataModel ToDataModel()
        {
            return new()
            {
                StartPosition = [StartPosition.X, StartPosition.Y, StartPosition.Z],
                Width = Width,
                Height = Height,
                XFacing = (int)XFacing,
                YFacing = (int)YFacing
            };
        }

        public static Screen FromDataModel(DataModel model)
        {
            return new(model);
        }

        public static Screen CreateScreen(Vector3<int> startPosition, Vector3<int> endPosition, Facing normalFacing)
        {
            Facing xFacing, yFacing;
            int width, height;
            if (startPosition.X == endPosition.X && (normalFacing == Facing.Xp || normalFacing == Facing.Xm))
            {
                bool swap = false;
                switch (normalFacing)
                {
                    case Facing.Xp:
                        if (startPosition.Z > endPosition.Z && startPosition.Y > endPosition.Y)
                        {
                            xFacing = Facing.Zm;
                            yFacing = Facing.Ym;
                        }
                        else if (startPosition.Z <= endPosition.Z && startPosition.Y <= endPosition.Y)
                        {
                            xFacing = Facing.Zp;
                            yFacing = Facing.Yp;
                        }
                        else if (startPosition.Z > endPosition.Z && startPosition.Y <= endPosition.Y)
                        {
                            swap = true;
                            xFacing = Facing.Yp;
                            yFacing = Facing.Zm;
                        }
                        else if (endPosition.Z <= endPosition.Z && startPosition.Y > endPosition.Y)
                        {
                            swap = true;
                            xFacing = Facing.Ym;
                            yFacing = Facing.Zp;
                        }
                        else
                        {
                            throw new InvalidOperationException();
                        }
                        break;
                    case Facing.Xm:
                        if (startPosition.Z > endPosition.Z && startPosition.Y > endPosition.Y)
                        {
                            swap = true;
                            xFacing = Facing.Ym;
                            yFacing = Facing.Zm;
                        }
                        else if (startPosition.Z <= endPosition.Z && startPosition.Y <= endPosition.Y)
                        {
                            swap = true;
                            xFacing = Facing.Yp;
                            yFacing = Facing.Zp;
                        }
                        else if (startPosition.Z > endPosition.Z && startPosition.Y <= endPosition.Y)
                        {
                            xFacing = Facing.Zm;
                            yFacing = Facing.Yp;
                        }
                        else if (endPosition.Z <= endPosition.Z && startPosition.Y > endPosition.Y)
                        {
                            xFacing = Facing.Zp;
                            yFacing = Facing.Ym;
                        }
                        else
                        {
                            throw new InvalidOperationException();
                        }
                        break;
                    default:
                        throw new InvalidOperationException();
                }

                if (swap)
                {
                    width = Math.Abs(startPosition.Y - endPosition.Y) + 1;
                    height = Math.Abs(startPosition.Z - endPosition.Z) + 1;
                }
                else
                {
                    width = Math.Abs(startPosition.Z - endPosition.Z) + 1;
                    height = Math.Abs(startPosition.Y - endPosition.Y) + 1;
                }
            }
            else if (startPosition.Y == endPosition.Y && (normalFacing == Facing.Yp || normalFacing == Facing.Ym))
            {
                bool swap = false;
                switch (normalFacing)
                {
                    case Facing.Yp:
                        if (startPosition.X > endPosition.X && startPosition.Z > endPosition.Z)
                        {
                            xFacing = Facing.Xm;
                            yFacing = Facing.Zm;
                        }
                        else if (startPosition.X <= endPosition.X && startPosition.Z <= endPosition.Z)
                        {
                            xFacing = Facing.Xp;
                            yFacing = Facing.Zp;
                        }
                        else if (startPosition.X > endPosition.X && startPosition.Z <= endPosition.Z)
                        {
                            swap = true;
                            xFacing = Facing.Zp;
                            yFacing = Facing.Xm;
                        }
                        else if (startPosition.X <= endPosition.X && startPosition.Z > endPosition.Z)
                        {
                            swap = true;
                            xFacing = Facing.Zm;
                            yFacing = Facing.Xp;
                        }
                        else
                        {
                            throw new InvalidOperationException();
                        }
                        break;
                    case Facing.Ym:
                        if (startPosition.X > endPosition.X && startPosition.Z > endPosition.Z)
                        {
                            swap = true;
                            xFacing = Facing.Zm;
                            yFacing = Facing.Xm;
                        }
                        else if (startPosition.X <= endPosition.X && startPosition.Z <= endPosition.Z)
                        {
                            swap = true;
                            xFacing = Facing.Zp;
                            yFacing = Facing.Xp;
                        }
                        else if (startPosition.X > endPosition.X && startPosition.Z <= endPosition.Z)
                        {
                            xFacing = Facing.Xm;
                            yFacing = Facing.Zp;
                        }
                        else if (startPosition.X <= endPosition.X && startPosition.Z > endPosition.Z)
                        {
                            xFacing = Facing.Xp;
                            yFacing = Facing.Zm;
                        }
                        else
                        {
                            throw new InvalidOperationException();
                        }
                        break;
                    default:
                        throw new InvalidOperationException();
                }

                if (swap)
                {
                    width = Math.Abs(startPosition.Z - endPosition.Z) + 1;
                    height = Math.Abs(startPosition.X - endPosition.X) + 1;
                }
                else
                {
                    width = Math.Abs(startPosition.X - endPosition.X) + 1;
                    height = Math.Abs(startPosition.Z - endPosition.Z) + 1;
                }
            }
            else if (startPosition.Z == endPosition.Z && (normalFacing == Facing.Zp || normalFacing == Facing.Zm))
            {
                bool swap = false;
                switch (normalFacing)
                {
                    case Facing.Zp:
                        if (startPosition.X > endPosition.X && startPosition.Y > endPosition.Y)
                        {
                            swap = true;
                            xFacing = Facing.Ym;
                            yFacing = Facing.Xm;
                        }
                        else if (startPosition.X <= endPosition.X && startPosition.Y <= endPosition.Y)
                        {
                            swap = true;
                            xFacing = Facing.Yp;
                            yFacing = Facing.Xp;
                        }
                        else if (startPosition.X > endPosition.X && startPosition.Y <= endPosition.Y)
                        {
                            xFacing = Facing.Xm;
                            yFacing = Facing.Yp;
                        }
                        else if (startPosition.X <= endPosition.X && startPosition.Y > endPosition.Y)
                        {
                            xFacing = Facing.Xp;
                            yFacing = Facing.Ym;
                        }
                        else
                        {
                            throw new InvalidOperationException();
                        }
                        break;
                    case Facing.Zm:
                        if (startPosition.X > endPosition.X && startPosition.Y > endPosition.Y)
                        {
                            xFacing = Facing.Xm;
                            yFacing = Facing.Ym;
                        }
                        else if (startPosition.X <= endPosition.X && startPosition.Y <= endPosition.Y)
                        {
                            xFacing = Facing.Xp;
                            yFacing = Facing.Yp;
                        }
                        else if (startPosition.X > endPosition.X && startPosition.Y <= endPosition.Y)
                        {
                            swap = true;
                            xFacing = Facing.Yp;
                            yFacing = Facing.Xm;
                        }
                        else if (startPosition.X <= endPosition.X && startPosition.Y > endPosition.Y)
                        {
                            swap = true;
                            xFacing = Facing.Ym;
                            yFacing = Facing.Xp;
                        }
                        else
                        {
                            throw new InvalidOperationException();
                        }
                        break;
                    default:
                        throw new InvalidOperationException();
                }

                if (swap)
                {
                    width = Math.Abs(startPosition.Y - endPosition.Y) + 1;
                    height = Math.Abs(startPosition.X - endPosition.X) + 1;
                }
                else
                {
                    width = Math.Abs(startPosition.X - endPosition.X) + 1;
                    height = Math.Abs(startPosition.Y - endPosition.Y) + 1;
                }
            }
            else
            {
                throw new ArgumentException("屏幕的起始点与截止点不在一个平面");
            }

            return new(startPosition, width, height, xFacing, yFacing);
        }

        public static bool operator ==(Screen left, Screen right)
        {
            return  left.StartPosition == right.StartPosition &&
                    left.Width == right.Width &&
                    left.Height == right.Height &&
                    left.XFacing == right.XFacing &&
                    left.YFacing == right.YFacing;
        }

        public static bool operator !=(Screen left, Screen right)
        {
            return !(left == right);
        }

        public class DataModel : IDataModel<DataModel>
        {
            public DataModel()
            {
                StartPosition = [0, 0, 0];
                Width = 128 + 32;
                Height = 72 + 32;
                XFacing = -1;
                YFacing = -2;
            }

            [Required(ErrorMessage = ErrorMessageHelper.RequiredAttribute)]
            [Length(3, 3, ErrorMessage = ErrorMessageHelper.LengthAttribute)]
            public int[] StartPosition { get; set; }

            [Range(1, 512, ErrorMessage = ErrorMessageHelper.RangeAttribute)]
            public int Width { get; set; }

            [Range(1, 512, ErrorMessage = ErrorMessageHelper.RangeAttribute)]
            public int Height { get; set; }

            [AllowedValues(1, -1, 2, -2, 3, -3, ErrorMessage = ErrorMessageHelper.AllowedValuesAttribute + "（值只能为 1, -1, 2, -2, 3, -3）")]
            public int XFacing { get; set; }

            [AllowedValues(1, -1, 2, -2, 3, -3, ErrorMessage = ErrorMessageHelper.AllowedValuesAttribute + "（值只能为 1, -1, 2, -2, 3, -3）")]
            public int YFacing { get; set; }

            public static DataModel CreateDefault()
            {
                return new();
            }

            public static void Validate(DataModel model, string name)
            {
                ValidationHelper.Validate(model, name);
            }
        }
    }
}
