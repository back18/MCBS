using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Drawing
{
    public class DrawReport
    {
        public required string Control { get; init; }

        public required string FrameType { get; init; }

        public required Size FrameSize { get; init; }

        public required bool IsRedraw { get; init; }

        public required double DrawingTime { get; init; }
    }
}
