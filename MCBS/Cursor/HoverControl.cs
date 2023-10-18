using MCBS.UI;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Cursor
{
    public class HoverControl
    {
        public HoverControl(IControl control)
        {
            Control = control ?? throw new ArgumentNullException(nameof(control));
            OffsetPosition = Point.Empty;
        }

        public HoverControl(IControl control, Point offsetPosition)
        {
            Control = control ?? throw new ArgumentNullException(nameof(control));
            OffsetPosition = offsetPosition;
        }

        public IControl Control { get; }

        public Point OffsetPosition { get; set; }
    }
}
