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
        public ScreenDrawingContext(BlockFrame baseLayer, BlockFrame cursorLayer, IDictionary<string, CursorDrawingContext> cursorDrawingContexts)
        {
            ArgumentNullException.ThrowIfNull(baseLayer, nameof(baseLayer));
            ArgumentNullException.ThrowIfNull(cursorLayer, nameof(cursorLayer));
            ArgumentNullException.ThrowIfNull(cursorDrawingContexts, nameof(cursorDrawingContexts));

            BaseLayer = baseLayer;
            CursorLayer = cursorLayer;
            CursorDrawingContexts = cursorDrawingContexts.AsReadOnly();
        }

        public BlockFrame BaseLayer { get; }

        public BlockFrame CursorLayer { get; }

        public ReadOnlyDictionary<string, CursorDrawingContext> CursorDrawingContexts { get; }
    }
}
