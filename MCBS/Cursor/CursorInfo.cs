using MCBS.Frame;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Cursor
{
    public class CursorInfo
    {
        public CursorInfo(string cursorType, Point offset, ArrayFrame frame)
        {
            if (string.IsNullOrEmpty(cursorType))
                throw new ArgumentException($"“{nameof(cursorType)}”不能为 null 或空。", nameof(cursorType));
            if (frame is null)
                throw new ArgumentNullException(nameof(frame));

            CursorType = cursorType;
            Offset = offset;
            Frame = frame;
        }

        public string CursorType { get; }

        public Point Offset { get; }

        public ArrayFrame Frame { get; }
    }
}
