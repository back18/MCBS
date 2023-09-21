using MCBS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Event
{
    public class ProcessEventArgs : EventArgs
    {
        public ProcessEventArgs(Process process)
        {
            Process = process;
        }

        public Process Process { get; }
    }
}
