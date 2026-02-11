using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.Services
{
    public interface ILogPathProvider
    {
        public DirectoryInfo Logs { get; }

        public FileInfo LatestLog { get; }

        public FileInfo DebugLog { get; }
    }
}
