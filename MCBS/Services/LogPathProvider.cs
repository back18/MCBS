using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.Services
{
    public class LogPathProvider : ILogPathProvider
    {
        public DirectoryInfo Logs => McbsPathManager.MCBS_Logs;

        public FileInfo LatestLog => McbsPathManager.MCBS_Logs_LatestLog;

        public FileInfo DebugLog => McbsPathManager.MCBS_Logs_DebugLog;
    }
}
