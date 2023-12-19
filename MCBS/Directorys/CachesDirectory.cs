using QuanLib.Core.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Directorys
{
    public class CachesDirectory : DirectoryBase
    {
        public CachesDirectory(string directory) : base(directory)
        {
            ColorMappingDir = AddDirectory<ColorMappingDirectory>("ColorMapping");
        }

        public ColorMappingDirectory ColorMappingDir { get; }
    }
}
