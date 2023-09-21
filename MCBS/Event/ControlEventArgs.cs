using MCBS.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Event
{
    public class ControlEventArgs<TControl> : EventArgs where TControl : class, IControl
    {
        public ControlEventArgs(TControl control)
        {
            Control = control;
        }

        public TControl Control { get; }
    }
}
