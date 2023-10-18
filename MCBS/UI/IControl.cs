using MCBS.Cursor;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.UI
{
    public interface IControl : IControlInitializeHandling, IControlEventHandling, IControlRendering, IComparable<IControl>
    {
        public IContainerControl? GenericParentContainer { get; }

        public string Text { get; set; }

        public bool IsHover { get; }

        public bool IsSelected { get; set; }

        public int DisplayPriority { get; set; }

        public CursorContext[] GetHoverCursors();

        public void SetGenericContainerControl(IContainerControl? container);
    }
}
