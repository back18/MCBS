using MCBS.Cursor;
using MCBS.Events;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.UI
{
    public interface IControl : IControlInitializeHandling, IControlEventHandling, IControlDrawing, IComparable<IControl>
    {
        public IContainerControl? ParentContainer { get; }

        public string Text { get; set; }

        public bool IsHover { get; }

        public bool IsSelected { get; set; }

        public int DisplayPriority { get; set; }

        public void UpdateHoverState(CursorEventArgs e);

        public CursorContext[] GetHoverCursors();

        public bool UpdateParentContainer(IContainerControl? parentContainer);
    }
}
