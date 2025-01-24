using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Drawing.Extensions
{
    public static class DrawResultExtensions
    {
        public static DrawReport ToDrawReport(this DrawResult source)
        {
            ArgumentNullException.ThrowIfNull(source, nameof(source));

            if (source is ContainerDrawResult containerDrawResult)
                return ToDrawReport(containerDrawResult);

            return new DrawReport()
            {
                Control = source.Control.ToString() ?? string.Empty,
                FrameType = source.BlockFrame.GetType().Name,
                FrameSize = new(source.BlockFrame.Width, source.BlockFrame.Height),
                IsRedraw = source.IsRedraw,
                DrawingTime = Math.Round(source.DrawingTime.TotalMilliseconds, 3, MidpointRounding.AwayFromZero)
            };
        }

        public static ContainerDrawReport ToDrawReport(this ContainerDrawResult source)
        {
            ArgumentNullException.ThrowIfNull(source, nameof(source));

            return new ContainerDrawReport()
            {
                Control = source.Control.ToString() ?? string.Empty,
                FrameType = source.BlockFrame.GetType().Name,
                FrameSize = new(source.BlockFrame.Width, source.BlockFrame.Height),
                IsRedraw = source.IsRedraw,
                DrawingTime = Math.Round(source.DrawingTime.TotalMilliseconds, 3, MidpointRounding.AwayFromZero),
                BackgroundDrawingTime = Math.Round(source.BackgroundDrawingTime.TotalMilliseconds, 3, MidpointRounding.AwayFromZero),
                ChildDrawingTime = Math.Round(source.ChildDrawingTime.TotalMilliseconds, 3, MidpointRounding.AwayFromZero),
                OverwriteDrawingTime = Math.Round(source.OverwriteDrawingTime.TotalMilliseconds, 3, MidpointRounding.AwayFromZero),
                ChildDrawReports = source.ChildDrawResults.Select(i => ToDrawReport(i)).ToArray().AsReadOnly()
            };
        }
    }
}
