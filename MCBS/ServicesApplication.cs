using MCBS.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS
{
    public abstract class ServicesApplication : Application
    {
        public abstract IRootForm RootForm { get; }
    }
}
