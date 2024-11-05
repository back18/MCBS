using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.BlockForms.FileSystem.IO
{
    public class IOList
    {
        public required IOPath[] FileIOPaths;

        public required IOPath[] DirectoryIOPaths;

        public required IOStream[] FileIOStreams;
    }
}
