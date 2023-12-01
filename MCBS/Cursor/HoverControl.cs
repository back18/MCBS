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
            ArgumentNullException.ThrowIfNull(control, nameof(control));

            Control = control;
            OffsetPosition = Point.Empty;
        }

        public HoverControl(IControl control, Point offsetPosition)
        {
            ArgumentNullException.ThrowIfNull(control, nameof(control));

            Control = control;
            OffsetPosition = offsetPosition;
        }

        public IControl Control { get; }

        public Point OffsetPosition { get; set; }
    }
}
