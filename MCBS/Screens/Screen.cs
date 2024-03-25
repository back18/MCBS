using QuanLib.Core;
using QuanLib.Game;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using QuanLib.DataAnnotations;

namespace MCBS.Screens
{
    /// <summary>
    /// 屏幕
    /// </summary>
    public class Screen : IPlane, IDataModelOwner<Screen, Screen.DataModel>
    {
        public Screen(DataModel model)
        {
            NullValidator.ValidateObject(model, nameof(model));

            StartPosition = new(model.StartPosition[0], model.StartPosition[1], model.StartPosition[2]);
            Width = model.Width;
            Height = model.Height;
            XFacing = (Facing)model.XFacing;
            YFacing = (Facing)model.YFacing;
        }

        public Screen(Vector3<int> startPosition, int width, int height, Facing xFacing, Facing yFacing)
        {
            StartPosition = startPosition;
            Width = width;
            Height = height;
            XFacing = xFacing;
            YFacing = yFacing;
        }

        public Vector3<int> StartPosition { get; internal set; }

        public Vector3<int> EndPosition => ScreenPos2WorldPos(new(Width - 1, Height - 1));

        public Vector3<int> CenterPosition => ScreenPos2WorldPos(new(Width / 2, Height / 2));

        public int Width { get; internal set; }

        public int Height { get; internal set; }

        public Facing XFacing { get; internal set; }

        public Facing YFacing { get; internal set; }

        public Facing NormalFacing => new PlaneFacing(XFacing, YFacing).NormalFacing;

        public PlaneAxis PlaneAxis
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

        public int PlaneCoordinate
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

        public int TotalPixels => Width * Height;

        public void UpRotate()
        {
            ApplyRotate(new PlaneFacing(XFacing, YFacing).UpRotate());
        }

        public void DownRotate()
        {
            ApplyRotate(new PlaneFacing(XFacing, YFacing).DownRotate());
        }

        public void LeftRotate()
        {
            ApplyRotate(new PlaneFacing(XFacing, YFacing).LeftRotate());
        }

        public void RightRotate()
        {
            ApplyRotate(new PlaneFacing(XFacing, YFacing).RightRotate());
        }

        public void ClockwiseRotate()
        {
            ApplyRotate(new PlaneFacing(XFacing, YFacing).ClockwiseRotate());
        }

        public void CounterclockwiseRotate()
        {
            ApplyRotate(new PlaneFacing(XFacing, YFacing).CounterclockwiseRotate());
        }

        private void ApplyRotate(PlaneFacing planeFacing)
        {
            Vector3<int> oldPos = CenterPosition;
            XFacing = planeFacing.XFacing;
            YFacing = planeFacing.YFacing;
            Vector3<int> newPos = CenterPosition;

            Vector3<int> offset = newPos - oldPos;
            StartPosition -= offset;
        }

        public void Translate(Point offset)
        {
            Translate(offset.X, offset.Y);
        }

        public void Translate(int dx, int dy)
        {
            Vector3<int> position = StartPosition;
            position = position.Offset((int)XFacing, dx);
            position = position.Offset((int)YFacing, dy);
            StartPosition = position;
        }

        public void OffsetPlaneCoordinate(int offset)
        {
            StartPosition = StartPosition.Offset((int)NormalFacing, offset);
        }

        public bool InAltitudeRange(int min, int max)
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

        public Screen SubScreen(Rectangle rectangle) => SubScreen(rectangle.Location, rectangle.Size);

        public Screen SubScreen(Point startPosition, Size size) => SubScreen(startPosition, size.Width, size.Height);

        public Screen SubScreen(Point startPosition, int width, int height)
        {
            return new(ScreenPos2WorldPos(startPosition), width, height, XFacing, YFacing);
        }

        public Vector3<int> ScreenPos2WorldPos(Point position, int offset = 0)
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
                    x = StartPosition.X + offset;
                    break;
                case Facing.Xm:
                    x = StartPosition.X - offset;
                    break;
                case Facing.Yp:
                    y = StartPosition.Y + offset;
                    break;
                case Facing.Ym:
                    y = StartPosition.Y - offset;
                    break;
                case Facing.Zp:
                    z = StartPosition.Z + offset;
                    break;
                case Facing.Zm:
                    z = StartPosition.Z - offset;
                    break;
                default:
                    break;
            }

            return new(x, y, z);
        }

        public Point WorldPos2ScreenPos(Vector3<int> position)
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

        public double GetPlaneDistance<T>(T position) where T : IVector3<double>
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

        public bool IncludedOnScreen(Point position)
        {
            return position.X >= 0 && position.Y >= 0 && position.X < Width && position.Y < Height;
        }

        public bool IncludedOnScreen(Vector3<int> position)
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

        public override string ToString()
        {
            return $"StartPosition={StartPosition}, Width={Width}, Height={Height}, XFacing={XFacing}, YFacing={YFacing}";
        }

        public DataModel ToDataModel()
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

        public class DataModel : IDataModel<DataModel>
        {
            public DataModel()
            {
                StartPosition = [0, 143, 0];
                Width = 256;
                Height = 144;
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
