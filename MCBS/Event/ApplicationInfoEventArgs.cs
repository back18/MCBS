using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Event
{
    public class ApplicationInfoEventArgs : EventArgs
    {
        public ApplicationInfoEventArgs(ApplicationInfo applicationInfo)
        {
            ApplicationInfo = applicationInfo;
        }

        public ApplicationInfo ApplicationInfo { get; }
    }
}
