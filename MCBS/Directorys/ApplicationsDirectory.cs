using QuanLib.Core.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Directorys
{
    public class ApplicationsDirectory : DirectoryBase
    {
        public ApplicationsDirectory(string directory) : base(directory)
        {
        }

        public string GetApplicationDirectory(string id)
        {
            ArgumentException.ThrowIfNullOrEmpty(id, nameof(id));

            return Combine(id);
        }
    }
}
