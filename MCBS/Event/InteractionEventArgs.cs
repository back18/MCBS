using MCBS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Event
{
    public class InteractionEventArgs : EventArgs
    {
        public InteractionEventArgs(Interaction interaction)
        {
            Interaction = interaction;
        }

        public Interaction Interaction { get; }
    }
}
