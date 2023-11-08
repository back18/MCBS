using MCBS.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.BlockForms
{
    public abstract class ContainerControl<TControl> : ContainerControl where TControl : Control
    {
        protected ContainerControl()
        {
            ChildControls = new(this);
        }

        public ControlCollection<TControl> ChildControls { get; }

        public override IReadOnlyControlCollection<Control> GetChildControls() => ChildControls;

        public override ControlCollection<T>? AsControlCollection<T>()
        {
            if (ChildControls is ControlCollection<T> result)
                return result;

            return null;
        }

        public override void ClearAllLayoutSyncer()
        {
            foreach (Control control in ChildControls)
            {
                control.ClearAllLayoutSyncer();
            }

            base.ClearAllLayoutSyncer();
        }
    }
}
