using MCBS.Screens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Events
{
    public class ScreenContextEventArgs : EventArgs
    {
        public ScreenContextEventArgs(ScreenContext screenContext)
        {
            ScreenContext = screenContext;
        }

        public ScreenContext ScreenContext { get; }
    }
}
