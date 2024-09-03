using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.SystemApplications.FileMoveHandler
{
    public readonly struct Throughput(TimeSpan timeSpan, long bytes)
    {
        private readonly double TicksPerSecond = TimeSpan.FromSeconds(1).Ticks;

        public TimeSpan TimeSpan { get; } = timeSpan;

        public long Bytes { get; } = bytes;

        public long SpeedPerSecond => (long)(Bytes * (TicksPerSecond / TimeSpan.Ticks));
    }
}
