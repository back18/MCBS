using QuanLib.Core.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Directorys
{
    public class SavesDirectory : DirectoryManager
    {
        public SavesDirectory(string directory) : base(directory)
        {
            InteractionsDir = AddDirectory<InteractionsDirectory>("Interactions");
            ScreenSavesFile = Combine("ScreenSaves.json");
        }

        public InteractionsDirectory InteractionsDir { get; }

        public string ScreenSavesFile { get; }
    }
}
