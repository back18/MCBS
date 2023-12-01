using MCBS.BlockForms;
using QuanLib.Core;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.BlockForms.Utility
{
    public static class LayoutHelper
    {
        public static void FillLayoutRightDown<T>(ContainerControl container, IEnumerable<T> controls, int spacing = 2) where T : Control
        {
            ArgumentNullException.ThrowIfNull(container, nameof(container));
            ArgumentNullException.ThrowIfNull(controls, nameof(controls));
            ThrowHelper.ArgumentOutOfMin(0, spacing, nameof(spacing));

            T? previous = null;
            foreach (var control in controls)
            {
                if (previous is null)
                    control.LayoutRight(container, spacing, spacing);
                else if (previous.ClientLocation.X + previous.Width + spacing + control.Width <= container.ClientSize.Width)
                    control.LayoutRight(container, previous, spacing);
                else
                    control.LayoutRight(container, previous.ClientLocation.Y + previous.Height + spacing, spacing);

                previous = control;
            }
        }

        public static void FillLayoutDownRight<T>(ContainerControl container, IEnumerable<T> controls, int spacing = 2) where T : Control
        {
            ArgumentNullException.ThrowIfNull(container, nameof(container));
            ArgumentNullException.ThrowIfNull(controls, nameof(controls));
            ThrowHelper.ArgumentOutOfMin(0, spacing, nameof(spacing));

            T? previous = null;
            foreach (var control in controls)
            {
                if (previous is null)
                    control.LayoutDown(container, spacing, spacing);
                else if (previous.ClientLocation.Y + previous.Height + spacing + control.Height <= container.ClientSize.Height)
                    control.LayoutDown(container, previous, spacing);
                else
                    control.LayoutDown(container, previous.ClientLocation.X + previous.Width + spacing, spacing);

                previous = control;
            }
        }
    }
}
