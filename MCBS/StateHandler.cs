using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS
{
    public delegate bool StateHandler<T>(T current, T next) where T : Enum;
}
