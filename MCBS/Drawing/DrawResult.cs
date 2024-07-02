using MCBS.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Drawing
{
    public class DrawResult : IComparable<DrawResult>
    {
        public DrawResult(IControl control, BlockFrame blockFrame, bool redraw, TimeSpan drawingTime)
        {
            ArgumentNullException.ThrowIfNull(control, nameof(control));
            ArgumentNullException.ThrowIfNull(blockFrame, nameof(blockFrame));

            Control = control;
            BlockFrame = blockFrame;
            IsRedraw = redraw;
            DrawingTime = drawingTime;
        }

        public IControl Control { get; }

        public BlockFrame BlockFrame { get; }

        public bool IsRedraw { get; }

        public TimeSpan DrawingTime { get; }

        public int CompareTo(DrawResult? other)
        {
            return Control.CompareTo(other?.Control);
        }
    }
}
