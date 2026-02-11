using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.Services
{
    public interface IMcbsPathProvider
    {
        public DirectoryInfo Config { get; }

        public DirectoryInfo Cache { get; }

        public DirectoryInfo Application { get; }

        public DirectoryInfo DllAppComponents { get; }

        public DirectoryInfo Logs { get; }

        public DirectoryInfo Minecraft { get; }

        public DirectoryInfo FFmpeg { get; }
    }
}
