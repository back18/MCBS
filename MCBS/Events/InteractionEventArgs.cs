using MCBS.Interaction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Events
{
    public class InteractionEventArgs : EventArgs
    {
        public InteractionEventArgs(InteractionContext interaction)
        {
            Interaction = interaction;
        }

        public InteractionContext Interaction { get; }
    }
}
