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

        public static Size GetDrawingSize(this IControlDrawing source)
        {
            ArgumentNullException.ThrowIfNull(source, nameof(source));

            return new(source.ClientSize.Width + source.BorderWidth * 2, source.ClientSize.Height + source.BorderWidth * 2);
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

        public static Point ChildPos2RootPos(this IControl source, Point position)
        {
            IControl? control = source;
            while (true)
            {
                position = ChildPos2ParentPos(control, position);
                control = source.ParentContainer;
                if (control is null)
                    break;
            }
            return position;
        }

        public static bool IncludedOnControl(this IControl source, Point position)
        {
            ArgumentNullException.ThrowIfNull(source, nameof(source));

            position.X -= source.OffsetPosition.X;
            position.Y -= source.OffsetPosition.Y;

            IContainerControl? parentContainer = source.ParentContainer;

            Point start = Point.Empty;
            Point end = new(source.ClientSize.Width - 1, source.ClientSize.Height - 1);

            if (parentContainer is not null)
            {
                Point startOffset = new(
                    source.ClientLocation.X + source.BorderWidth - parentContainer.OffsetPosition.X,
                    source.ClientLocation.Y + source.BorderWidth - parentContainer.OffsetPosition.Y);

                if (startOffset.X < 0)
                    start.X -= startOffset.X;
                if (startOffset.Y < 0)
                    start.Y -= startOffset.Y;

                Point endOffset = new(
                    parentContainer.ClientSize.Width - (source.ClientLocation.X + source.ClientSize.Width + source.BorderWidth) + parentContainer.OffsetPosition.X,
                    parentContainer.ClientSize.Height - (source.ClientLocation.Y + source.ClientSize.Height + source.BorderWidth) + parentContainer.OffsetPosition.Y);

                if (endOffset.X < 0)
                    end.X += endOffset.X;
                if (endOffset.Y < 0)
                    end.Y += endOffset.Y;
            }

            return
                position.X >= start.X &&
                position.Y >= start.Y &&
                position.X <= end.X &&
                position.Y <= end.Y;
        }

        public static IControl[] GetVisibleChildControls(this IContainerControl source, bool recursive = false)
        {
            IControl[] controls = source
                .GetChildControls()
                .Where(w =>
                w.Visible &&
                w.ClientSize.Width > 0 &&
                w.ClientSize.Height > 0 &&
                w.ClientLocation.X + w.ClientSize.Width + w.BorderWidth > source.OffsetPosition.X &&
                w.ClientLocation.Y + w.ClientSize.Height + w.BorderWidth > source.OffsetPosition.Y &&
                w.ClientLocation.X - w.BorderWidth < source.ClientSize.Width + source.OffsetPosition.X &&
                w.ClientLocation.Y - w.BorderWidth < source.ClientSize.Height + source.OffsetPosition.Y)
                .ToArray();

            if (!recursive || controls.Length == 0)
                return controls;

            List<IControl> list = new(controls);
            foreach (var control in controls)
            {
                if (control is IContainerControl containerControl)
                    list.AddRange(GetVisibleChildControls(containerControl, true));
            }

            return list.ToArray();
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
