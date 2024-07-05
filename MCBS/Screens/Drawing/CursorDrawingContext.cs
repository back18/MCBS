using MCBS.Cursor;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Screens.Drawing
{
    public class CursorDrawingContext : IEquatable<CursorDrawingContext>
    {
        public CursorDrawingContext(Point cursorPosition, string styleType, bool visible, IList<HoverControl> hoverControls)
        {
            ArgumentException.ThrowIfNullOrEmpty(styleType, nameof(styleType));
            ArgumentNullException.ThrowIfNull(hoverControls, nameof(hoverControls));

            CursorPosition = cursorPosition;
            StyleType = styleType;
            Visible = visible;
            HoverControls = hoverControls.AsReadOnly();
        }

        public Point CursorPosition { get; }

        public string StyleType { get; }

        public bool Visible { get; }

        public ReadOnlyCollection<HoverControl> HoverControls { get; }

        public bool IsRequestRedraw => HoverControls.Where(w => w.Control.IsRequestRedraw).Any();

        public override int GetHashCode()
        {
            return HashCode.Combine(CursorPosition.X, CursorPosition.Y, StyleType, Visible, HoverControls.GetHashCode());
        }

        public override bool Equals(object? obj)
        {
            return obj is CursorDrawingContext other && Equals(other);
        }

        public bool Equals(CursorDrawingContext? other)
        {
            if (other is null)
                return false;

            return
                CursorPosition == other.CursorPosition &&
                StyleType == other.StyleType &&
                Visible == other.Visible &&
                Equals(HoverControls, other.HoverControls);
        }

        private static bool Equals<T>(IReadOnlyCollection<T> list1, IReadOnlyCollection<T> list2) where T : notnull
        {
            ArgumentNullException.ThrowIfNull(list1, nameof(list1));
            ArgumentNullException.ThrowIfNull(list2, nameof(list2));

            if (list1.Count != list2.Count)
                return false;

            foreach (var item in list1)
            {
                if (!list2.Contains(item))
                    return false;
            }

            return true;
        }
    }
}
