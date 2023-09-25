using QuanLib.Core.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Directorys
{
    public class VersionDircetory : DirectoryManager
    {
        public VersionDircetory(string directory) : base(directory)
        {
            Languages = new(Combine("Languages"));
            Client = new(Combine("client.jar"));
            Version = new(Combine("version.json"));
            Index = new(Combine("index.json"));
        }

        public LanguagesDirectory Languages { get; }

        public string Client { get; }

        public string Version { get; }

        public string Index { get; }
    }
}
