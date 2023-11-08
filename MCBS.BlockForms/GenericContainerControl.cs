using MCBS.BlockForms.Utility;
using MCBS.Events;
using MCBS.UI;
using QuanLib.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.BlockForms
{
    public abstract partial class GenericContainerControl : AbstractControlContainer<IControl>
    {
        protected GenericContainerControl()
        {
            AddedChildControl += (sender, e) => { };
            RemovedChildControl += (sender, e) => { };
        }

        public abstract ControlCollection<T>? AsControlCollection<T>() where T : class, IControl;

        public override event EventHandler<AbstractControlContainer<IControl>, ControlEventArgs<IControl>> AddedChildControl;

        public override event EventHandler<AbstractControlContainer<IControl>, ControlEventArgs<IControl>> RemovedChildControl;
    }
}
