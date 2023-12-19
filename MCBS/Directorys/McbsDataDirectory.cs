using QuanLib.Core.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Directorys
{
    public class McbsDataDirectory : DirectoryBase
    {
        public McbsDataDirectory(string directory) : base(directory)
        {
            InteractionsDir = AddDirectory<InteractionsDirectory>("Interactions");
            ScreenDataFile = Combine("ScreenData.json");
        }

        public InteractionsDirectory InteractionsDir { get; }

        public string ScreenDataFile { get; }
    }
}
