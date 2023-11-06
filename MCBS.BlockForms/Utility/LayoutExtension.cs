using QuanLib.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.BlockForms.Utility
{
    public static class LayoutExtension
    {
        #region 根据目标控件布局

        public static void LayoutUp(this Control source, ContainerControl container, Control control, int spacing = 2)
        {
            if (container is null)
                throw new ArgumentNullException(nameof(container));
            if (control is null)
                throw new ArgumentNullException(nameof(control));
            ThrowHelper.ArgumentOutOfMin(0, spacing, nameof(spacing));

            source.ClientLocation = new(control.ClientLocation.X, control.ClientLocation.Y - source.Height - spacing);
        }

        public static void LayoutDown(this Control source, ContainerControl container, Control control, int spacing = 2)
        {
            if (container is null)
                throw new ArgumentNullException(nameof(container));
            if (control is null)
                throw new ArgumentNullException(nameof(control));
            ThrowHelper.ArgumentOutOfMin(0, spacing, nameof(spacing));

            source.ClientLocation = new(control.ClientLocation.X, control.ClientLocation.Y + control.Height + spacing);
        }

        public static void LayoutLeft(this Control source, ContainerControl container, Control control, int spacing = 2)
        {
            if (container is null)
                throw new ArgumentNullException(nameof(container));
            if (control is null)
                throw new ArgumentNullException(nameof(control));
            ThrowHelper.ArgumentOutOfMin(0, spacing, nameof(spacing));

            source.ClientLocation = new(control.ClientLocation.X - source.Width - spacing, control.ClientLocation.Y);
        }

        public static void LayoutRight(this Control source, ContainerControl container, Control control, int spacing = 2)
        {
            if (container is null)
                throw new ArgumentNullException(nameof(container));
            if (control is null)
                throw new ArgumentNullException(nameof(control));
            ThrowHelper.ArgumentOutOfMin(0, spacing, nameof(spacing));

            source.ClientLocation = new(control.ClientLocation.X + control.Width + spacing, control.ClientLocation.Y);
        }

        #endregion

        #region 根据目标位置布局

        public static void LayoutUp(this Control source, ContainerControl container, int location, int spacing = 2)
        {
            if (container is null)
                throw new ArgumentNullException(nameof(container));
            ThrowHelper.ArgumentOutOfMin(0, spacing, nameof(spacing));

            source.ClientLocation = new(location, container.ClientSize.Height - source.Height - spacing);
        }

        public static void LayoutDown(this Control source, ContainerControl container, int location, int spacing = 2)
        {
            if (container is null)
                throw new ArgumentNullException(nameof(container));
            ThrowHelper.ArgumentOutOfMin(0, spacing, nameof(spacing));

            source.ClientLocation = new(location, spacing);
        }

        public static void LayoutLeft(this Control source, ContainerControl container, int location, int spacing = 2)
        {
            if (container is null)
                throw new ArgumentNullException(nameof(container));
            ThrowHelper.ArgumentOutOfMin(0, spacing, nameof(spacing));

            source.ClientLocation = new(container.ClientSize.Width - source.Width - spacing, location);
        }

        public static void LayoutRight(this Control source, ContainerControl container, int location, int spacing = 2)
        {
            if (container is null)
                throw new ArgumentNullException(nameof(container));
            ThrowHelper.ArgumentOutOfMin(0, spacing, nameof(spacing));

            source.ClientLocation = new(spacing, location);
        }

        #endregion

        #region 居中布局

        public static void LayoutCentered(this Control source, ContainerControl container)
        {
            if (container is null)
                throw new ArgumentNullException(nameof(container));

            source.ClientLocation = new(container.ClientSize.Width / 2 - source.ClientSize.Width / 2, container.ClientSize.Height / 2 - source.ClientSize.Height / 2);
        }

        public static void LayoutVerticalCentered(this Control source, ContainerControl container, int location)
        {
            if (container is null)
                throw new ArgumentNullException(nameof(container));

            source.ClientLocation = new(location, container.ClientSize.Height / 2 - source.ClientSize.Height / 2);
        }

        public static void LayoutHorizontalCentered(this Control source, ContainerControl container, int location)
        {
            if (container is null)
                throw new ArgumentNullException(nameof(container));

            source.ClientLocation = new(container.ClientSize.Width / 2 - source.ClientSize.Width / 2, location);
        }

        #endregion

        #region 填充布局



        #endregion
    }
}
