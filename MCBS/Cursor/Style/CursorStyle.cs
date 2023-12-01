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
            ArgumentException.ThrowIfNullOrEmpty(cursorType, nameof(cursorType));
            ArgumentNullException.ThrowIfNull(blockFrame, nameof(blockFrame));

            CursorType = cursorType;
            Offset = offset;
            BlockFrame = blockFrame;
        }

        public string CursorType { get; }

        public Point Offset { get; }

        public BlockFrame BlockFrame { get; }
    }
}
