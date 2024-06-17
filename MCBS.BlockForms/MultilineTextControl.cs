using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.BlockForms
{
    public abstract class MultilineTextControl : AbstractMultilineTextControl
    {
        protected MultilineTextControl()
        {
            BlockResolution = 1;
            FontPixelSize = 1;
            ScrollDelta = SR.DefaultFont.Height / BlockResolution * FontPixelSize;
        }

        protected override string ToBlockId(int index)
        {
            return (index == 0 ? GetBackgroundColor() : GetForegroundColor()).ToBlockId();
        }
    }
}
