using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.BlockForms.FileSystem
{
    public readonly struct ProgressTimestamp(DateTime dateTime, long bytes)
    {
        public readonly DateTime DateTime = dateTime;

        public readonly long Bytes = bytes;

        public static Throughput operator -(ProgressTimestamp a, ProgressTimestamp b)
        {
            return new(a.DateTime - b.DateTime, a.Bytes - b.Bytes);
        }
    }
}
