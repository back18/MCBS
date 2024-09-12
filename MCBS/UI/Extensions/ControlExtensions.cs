using MCBS.Events;
using MCBS.Screens;
using MCBS.UI.Extensions;
using QuanLib.Game;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.UI.Extensions
{
    public static class ControlExtensions
    {
        public static int IndexOf<T>(this IReadOnlyControlCollection<T> source, T item) where T : class, IControl
        {
            ArgumentNullException.ThrowIfNull(source, nameof(source));
            ArgumentNullException.ThrowIfNull(item, nameof(item));

            for (int i = 0; i < source.Count; i++)
            {
                if (source[i] == item)
                    return i;
            }
            return -1;
        }

        public static IForm? GetForm(this IControl source)
        {
            ArgumentNullException.ThrowIfNull(source, nameof(source));

            if (source is IForm result)
                return result;

            IControl? parent = source.ParentContainer;
            if (parent is not null)
                return GetForm(parent);

            return null;
        }

        public static IRootForm? GetRootForm(this IControl source)
        {
            ArgumentNullException.ThrowIfNull(source, nameof(source));

            if (source is IRootForm result)
                return result;

            IControl? parent = source.ParentContainer;
            if (parent is not null)
                return GetRootForm(parent);

            return null;
        }

        public static IScreenView? GetScreenView(this IControl source)
        {
            ArgumentNullException.ThrowIfNull(source, nameof(source));

            if (source is IScreenView result)
                return result;

            IControl? parent = source.ParentContainer;
            if (parent is not null)
                return GetScreenView(parent);

            return null;
        }

        public static IControl GetRootControl(this IControl source)
        {
            ArgumentNullException.ThrowIfNull(source, nameof(source));

            IControl? parent = source.ParentContainer;
            if (parent is not null)
                return GetRootControl(parent);

            return source;
        }

        public static Rectangle GetRectangle(this IControl source)
        {
            ArgumentNullException.ThrowIfNull(source, nameof(source));

            return new(source.ClientLocation.X, source.ClientLocation.Y, source.ClientSize.Width + source.BorderWidth, source.ClientSize.Height + source.BorderWidth);
        }

        public static Facing GetNormalFacing(this IControl source)
        {
            ArgumentNullException.ThrowIfNull(source, nameof(source));

            return GetScreenContext(source)?.Screen.NormalFacing ?? Facing.Zm;
        }

        public static ScreenContext? GetScreenContext(this IControl source)
        {
            ArgumentNullException.ThrowIfNull(source, nameof(source));

            IForm? form = GetForm(source);
            if (form is null)
                return null;

            return MinecraftBlockScreen.Instance.ScreenContextOf(form);
        }

        public static Point GetDrawingLocation(this IControlDrawing source)
        {
            ArgumentNullException.ThrowIfNull(source, nameof(source));

            return new(source.ClientLocation.X + source.BorderWidth, source.ClientLocation.Y + source.BorderWidth);
        }

        public static Point ParentPos2ChildPos(this IControl source, Point position)
        {
            ArgumentNullException.ThrowIfNull(source, nameof(source));

            return new(position.X - source.ClientLocation.X + source.OffsetPosition.X - source.BorderWidth, position.Y - source.ClientLocation.Y + source.OffsetPosition.Y - source.BorderWidth);
        }

        public static Point ChildPos2ParentPos(this IControl source, Point position)
        {
            ArgumentNullException.ThrowIfNull(source, nameof(source));

            return new(position.X + source.ClientLocation.X + source.OffsetPosition.X + source.BorderWidth, position.Y + source.ClientLocation.Y + source.OffsetPosition.Y + source.BorderWidth);
        }

        public static bool IncludedOnControl(this IControl source, Point position)
        {
            ArgumentNullException.ThrowIfNull(source, nameof(source));

            position.X -= source.OffsetPosition.X;
            position.Y -= source.OffsetPosition.Y;
            return position.X >= 0 && position.Y >= 0 && position.X < source.ClientSize.Width && position.Y < source.ClientSize.Height;
        }

        public static void UpdateAllHoverState(this IContainerControl source, CursorEventArgs e)
        {
            ArgumentNullException.ThrowIfNull(source, nameof(source));
            ArgumentNullException.ThrowIfNull(e, nameof(e));

            foreach (var control in source.GetChildControls().ToArray())
            {
                if (control is IContainerControl containerControl)
                    containerControl.UpdateAllHoverState(e.Clone(control.ParentPos2ChildPos));
                else
                    control.UpdateHoverState(e.Clone(control.ParentPos2ChildPos));
            }

            source.UpdateHoverState(e);
        }
    }
}
