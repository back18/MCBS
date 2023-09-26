using QuanLib.Core.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Directorys
{
    public class LogsDirectory : DirectoryManager
    {
        public LogsDirectory(string directory) : base(directory)
        {
            LatestFile = Combine("Latest.log");
        }

        public string LatestFile { get; }
    }
}
