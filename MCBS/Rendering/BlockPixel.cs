using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Rendering
{
    public abstract class BlockPixel
    {
        public abstract string ToBlockId();

        public abstract BlockPixel Clone();

        public static bool Equals(BlockPixel? blockPixel1, BlockPixel? blockPixel2)
        {
            if (blockPixel1 == blockPixel2)
                return true;
            if (blockPixel1 is null || blockPixel2 is null)
                return false;

            return blockPixel1.ToBlockId() == blockPixel2.ToBlockId();
        }
    }
}
