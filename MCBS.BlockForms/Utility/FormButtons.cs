using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.BlockForms.Utility
{
    [Flags]
    public enum FormButtons
    {
        None = 0,

        Close = 1,

        MaximizeOrRestore = 2,

        Minimize = 4,

        FullScreen = 8
    }
}
