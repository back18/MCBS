using MCBS.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.BlockForms
{
    public abstract class GenericContainerControl<TControl> : GenericContainerControl where TControl : class, IControl
    {
        protected GenericContainerControl()
        {
            ChildControls = new(this);
        }

        public ControlCollection<TControl> ChildControls { get; }

        public override IReadOnlyControlCollection<IControl> GetChildControls() => ChildControls;

        public override ControlCollection<T>? AsControlCollection<T>()
        {
            if (ChildControls is ControlCollection<T> result)
                return result;

            return null;
        }
    }
}
