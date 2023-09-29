using MCBS.BlockForms.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.BlockForms
{
    public class ListMenuBox<T> : MenuBox<T> where T : Control
    {
        public override void AddedChildControlAndLayout(T control)
        {
            ChildControls.Add(control);
            if (PreviousChildControl is not null)
                control.ClientLocation = this.BottomLayout(PreviousChildControl, Spacing);
            else
                control.ClientLocation = new(Spacing, Spacing);
            _items.Add(control);

            PageSize = new(ClientSize.Width, control.BottomLocation + 1 + Spacing);
        }

        public override void RemoveChildControlAndLayout(T control)
        {
            throw new NotImplementedException();
        }
    }
}
