using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Drawing
{
    public class ContainerDrawReport : DrawReport
    {
        public required double BackgroundDrawingTime { get; init; }

        public required double ChildDrawingTime { get; init; }

        public required double OverwriteDrawingTime { get; init; }

        public required ReadOnlyCollection<DrawReport> ChildDrawReports { get; init; }
    }
}
