using MCBS.Rendering;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Cursor.Style
{
    public class CursorStyle
    {
        public CursorStyle(string cursorType, Point offset, BlockFrame blockFrame)
        {
            if (string.IsNullOrEmpty(cursorType))
                throw new ArgumentException($"“{nameof(cursorType)}”不能为 null 或空。", nameof(cursorType));
            if (blockFrame is null)
                throw new ArgumentNullException(nameof(blockFrame));

            CursorType = cursorType;
            Offset = offset;
            BlockFrame = blockFrame;
        }

        public string CursorType { get; }

        public Point Offset { get; }

        public BlockFrame BlockFrame { get; }
    }
}
