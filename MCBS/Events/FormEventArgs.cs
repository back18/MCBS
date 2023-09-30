using MCBS.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Events
{
    public class FormEventArgs : EventArgs
    {
        public FormEventArgs(IForm form)
        {
            Form = form;
        }

        public IForm Form { get; }
    }
}
