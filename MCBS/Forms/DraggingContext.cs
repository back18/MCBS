using MCBS.Cursor;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Forms
{
    public class DraggingContext
    {
        public DraggingContext(CursorContext cursorContext, Point offsetPosition)
        {
            CursorContext = cursorContext ?? throw new ArgumentNullException(nameof(cursorContext));
            OffsetPosition = offsetPosition;
        }

        public CursorContext CursorContext { get; }

        public Point OffsetPosition { get; }
    }
}
