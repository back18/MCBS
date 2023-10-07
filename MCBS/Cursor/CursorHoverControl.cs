using MCBS.UI;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Cursor
{
    public class CursorHoverControl
    {
        public CursorHoverControl(Point offset, IControl control)
        {
            if (control is null)
                throw new ArgumentNullException(nameof(control));

            OffsetPosition = offset;
            Control = control;
        }

        public Point OffsetPosition { get; }

        public IControl Control { get; }
    }
}
