using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.UI
{
    public interface IButton : IControl
    {
        public int ReboundTime { get; set; }

        public int ReboundCountdown { get; }
    }
}
