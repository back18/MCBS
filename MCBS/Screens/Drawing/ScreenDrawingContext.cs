using MCBS.Drawing;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Screens.Drawing
{
    public class ScreenDrawingContext
    {
        public ScreenDrawingContext(BlockFrame blockFrame, IDictionary<string, CursorDrawingContext> cursorDrawingContexts)
        {
            ArgumentNullException.ThrowIfNull(blockFrame, nameof(blockFrame));
            ArgumentNullException.ThrowIfNull(cursorDrawingContexts, nameof(cursorDrawingContexts));

            BlockFrame = blockFrame;
            CursorDrawingContexts = cursorDrawingContexts.AsReadOnly();
        }

        public BlockFrame BlockFrame { get; }

        public ReadOnlyDictionary<string, CursorDrawingContext> CursorDrawingContexts { get; }
    }
}
