using QuanLib.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Directorys
{
    public class VanillaDirectory : DirectoryBase
    {
        public VanillaDirectory(string directory) : base(directory)
        {
        }

        public VersionDirectory GetVersionDirectory(string version)
        {
            ArgumentException.ThrowIfNullOrEmpty(version, nameof(version));

            return new VersionDirectory(Combine(version));
        }
    }
}
