using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.UI
{
    public interface IScreenView : IControl
    {
        public IRootForm RootForm { get; }
    }
}
