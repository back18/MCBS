using MCBS.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Application
{
    public interface IServicesProgram : IProgram
    {
        public abstract IRootForm RootForm { get; }

        public abstract IScreenView ScreenView { get; }
    }
}
