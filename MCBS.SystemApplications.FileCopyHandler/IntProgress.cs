using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.SystemApplications.FileCopyHandler
{
    public readonly struct IntProgress(int total, int completed) : IEquatable<IntProgress>
    {
        public int Total { get; } = total;

        public int Completed { get; } = completed;

        public double Progress => Total == 0 ? 0 : Completed / (double)Total;

        public bool Equals(IntProgress other)
        {
            return Total == other.Total && Completed == other.Completed;
        }

        public override bool Equals(object? obj)
        {
            return obj is IntProgress other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Total, Completed);
        }

        public static bool operator ==(IntProgress left, IntProgress right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(IntProgress left, IntProgress right)
        {
            return !left.Equals(right);
        }
    }
}
