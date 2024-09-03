using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.SystemApplications.FileDeleteHandler
{
    public readonly struct ViewData(IntProgress fileCount) : IEquatable<ViewData>
    {
        public readonly IntProgress FileCount = fileCount;

        public bool Equals(ViewData other)
        {
            return FileCount == other.FileCount;
        }

        public override bool Equals(object? obj)
        {
            return obj is ViewData other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(FileCount);
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
