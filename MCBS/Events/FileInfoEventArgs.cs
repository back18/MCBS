using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Events
{
    public class FIleInfoEventArgs : EventArgs
    {
        public FIleInfoEventArgs(FileInfo fileInfo)
        {
            FileInfo = fileInfo;
        }

        public FileInfo FileInfo { get; }
    }
}
