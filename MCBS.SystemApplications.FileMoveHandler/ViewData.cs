using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.SystemApplications.FileMoveHandler
{
    public readonly struct ViewData(IntProgress fileCount, LongProgress totalFileBytes, LongProgress currentFileBytes) : IEquatable<ViewData>
    {
        public readonly IntProgress FileCount = fileCount;

        public readonly LongProgress TotalFileBytes = totalFileBytes;

        public readonly LongProgress CurrentFileBytes = currentFileBytes;

        public bool Equals(ViewData other)
        {
            return FileCount == other.FileCount && TotalFileBytes == other.TotalFileBytes && CurrentFileBytes == other.CurrentFileBytes;
        }

        public override bool Equals(object? obj)
        {
            return obj is ViewData other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(FileCount, TotalFileBytes, CurrentFileBytes);
        }

        public static bool operator ==(ViewData left, ViewData right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ViewData left, ViewData right)
        {
            return !left.Equals(right);
        }
    }
}
