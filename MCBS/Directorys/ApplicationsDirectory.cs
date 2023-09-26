using QuanLib.Core.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Directorys
{
    public class ApplicationsDirectory : DirectoryManager
    {
        public ApplicationsDirectory(string directory) : base(directory)
        {
        }

        public string GetApplicationDirectory(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException($"“{nameof(id)}”不能为 null 或空。", nameof(id));

            return Combine(id);
        }
    }
}
