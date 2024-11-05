using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.BlockForms.FileSystem
{
    public readonly struct LongProgress(long total, long completed) : IEquatable<LongProgress>
    {
        public long Total { get; } = total;

        public long Completed { get; } = completed;

        public double Progress => Total == 0 ? 0 : Completed / (double)Total;

        public bool Equals(LongProgress other)
        {
            return Total == other.Total && Completed == other.Completed;
        }

        public override bool Equals(object? obj)
        {
            return obj is LongProgress other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Total, Completed);
        }

        public static bool operator ==(LongProgress left, LongProgress right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(LongProgress left, LongProgress right)
        {
            return !left.Equals(right);
        }
    }
}
