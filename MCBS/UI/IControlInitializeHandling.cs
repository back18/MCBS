using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.UI
{
    public interface IControlInitializeHandling
    {
        public bool IsInitCompleted { get; }

        public void HandleInitialize();

        public void HandleBeforeInitialize();

        public void HandleAfterInitialize();
    }
}
