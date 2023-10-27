using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Rendering
{
    public interface IBlockMapping<TPixel> : IReadOnlyDictionary<TPixel, string>
    {

    }
}
