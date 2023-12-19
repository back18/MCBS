using QuanLib.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Directorys
{
    public class VersionDirectory : DirectoryBase
    {
        public VersionDirectory(string directory) : base(directory)
        {
            LanguagesDir = AddDirectory<LanguagesDirectory>("Languages");
            ClientFile = Combine("client.jar");
            VersionFile = Combine("version.json");
            IndexFile = Combine("index.json");
        }

        public LanguagesDirectory LanguagesDir { get; }

        public string ClientFile { get; }

        public string VersionFile { get; }

        public string IndexFile { get; }
    }
}
