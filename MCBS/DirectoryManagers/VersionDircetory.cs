using QuanLib.Core.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.DirectoryManagers
{
    public class VersionDircetory : DirectoryManager
    {
        public VersionDircetory(string directory) : base(directory)
        {
            Languages = new(Combine("Languages"));
            Client = new(Combine("client.jar"));
        }

        public LanguagesDirectory Languages { get; }

        public string Client { get; }
    }
}
