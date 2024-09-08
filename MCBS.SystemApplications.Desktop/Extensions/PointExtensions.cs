using QuanLib.Core;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.SystemApplications.Desktop.Extensions
{
    public static class PointExtensions
    {
        public static string ToPositionString(this Point point)
        {
            return $"[{point.X},{point.Y}]";
        }

        public static string ToHex8(this Point point)
        {
            return Convert.ToString((byte)point.X, 16).PadLeft(2, '0') + Convert.ToString((byte)point.Y, 16).PadLeft(2, '0');
        }

        public static Point ParseHex8(string s)
        {
            ThrowHelper.StringLengthOutOfRange(4, s, nameof(s));

            int x = Convert.ToInt32(s[..2], 16);
            int y = Convert.ToInt32(s[2..], 16);
            return new Point(x, y);
        }
    }
}
