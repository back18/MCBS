using MCBS.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Drawing
{
    public class ContainerDrawResult : DrawResult
    {
        public ContainerDrawResult(
            IControl control,
            BlockFrame blockFrame,
            bool redraw,
            TimeSpan drawingTime,
            TimeSpan backgroundDrawingTime,
            TimeSpan childDrawingTime,
            TimeSpan overwriteDrawingTime,
            IList<DrawResult> childDrawResults)
            : base(control, blockFrame, redraw, drawingTime)
        {
            ArgumentNullException.ThrowIfNull(childDrawResults, nameof(childDrawResults));

            BackgroundDrawingTime = backgroundDrawingTime;
            ChildDrawingTime = childDrawingTime;
            OverwriteDrawingTime = overwriteDrawingTime;
            ChildDrawResults = childDrawResults.AsReadOnly();
        }

        public TimeSpan BackgroundDrawingTime { get; }

        public TimeSpan ChildDrawingTime { get; }

        public TimeSpan OverwriteDrawingTime { get; }

        public ReadOnlyCollection<DrawResult> ChildDrawResults { get; }
    }
}
