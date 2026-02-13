using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.Common
{
    public readonly struct BuildProgress(int totalCount, int completedCount)
    {
        public int TotalCount { get; } = totalCount;

        public int CompletedCount { get; } = completedCount;

        public double ProgressPercentage => TotalCount == 0 ? 0 : ((double)CompletedCount * 100) / TotalCount;
    }
}
