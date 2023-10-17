using QuanLib.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.UI
{
    public interface IFormEventHandling
    {
        public void HandleFormLoad(EventArgs e);

        public void HandleFormClose(EventArgs e);

        public void HandleFormMinimize(EventArgs e);

        public void HandleFormUnminimize(EventArgs e);
    }
}
