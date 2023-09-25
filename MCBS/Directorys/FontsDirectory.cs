using QuanLib.Core.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Directorys
{
    public class FontsDirectory : DirectoryManager
    {
        public FontsDirectory(string directory) : base(directory)
        {
            DefaultFont = Combine("DefaultFont.bdf");
        }

        public string DefaultFont { get; }
    }
}
