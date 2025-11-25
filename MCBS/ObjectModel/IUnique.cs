using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.ObjectModel
{
    public interface IUnique
    {
        public Guid Guid { get; }

        public string ShortId { get; }
    }
}
