using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.SystemApplications.FileCopyHandler
{
    public class IOStream
    {
        public IOStream(FileStream source, FileStream destination)
        {
            ArgumentNullException.ThrowIfNull(source, nameof(source));
            ArgumentNullException.ThrowIfNull(destination, nameof(destination));

            Source = source;
            Destination = destination;
        }

        public FileStream Source { get; }

        public FileStream Destination { get; }
    }
}
