using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.BlockForms.FileSystem.IO
{
    public readonly struct IOPath(string source, string destination)
    {
        public readonly string Source = source;

        public readonly string Destination = destination;
    }
}
