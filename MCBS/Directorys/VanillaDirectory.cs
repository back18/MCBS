using QuanLib.Core.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Directorys
{
    public class VanillaDirectory : DirectoryManager
    {
        public VanillaDirectory(string directory) : base(directory)
        {
        }

        public VersionDirectory GetVersionDirectory(string version)
        {
            if (string.IsNullOrEmpty(version))
                throw new ArgumentException($"“{nameof(version)}”不能为 null 或空。", nameof(version));

            return new VersionDirectory(Combine(version));
        }
    }
}
