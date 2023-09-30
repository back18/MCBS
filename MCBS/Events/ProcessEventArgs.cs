using MCBS.Processes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Events
{
    public class ProcessEventArgs : EventArgs
    {
        public ProcessEventArgs(ProcessContext process)
        {
            Process = process;
        }

        public ProcessContext Process { get; }
    }
}
