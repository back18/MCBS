using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.Services
{
    public class McbsPathProvider : IMcbsPathProvider
    {
        public DirectoryInfo Config => McbsPathManager.MCBS_Config;

        public DirectoryInfo Cache => McbsPathManager.MCBS_Cache;

        public DirectoryInfo Application => McbsPathManager.MCBS_Application;

        public DirectoryInfo DllAppComponents => McbsPathManager.MCBS_DllAppComponents;

        public DirectoryInfo Logs => McbsPathManager.MCBS_Logs;

        public DirectoryInfo Minecraft => McbsPathManager.MCBS_Minecraft;

        public DirectoryInfo FFmpeg => McbsPathManager.MCBS_FFmpeg;
    }
}
