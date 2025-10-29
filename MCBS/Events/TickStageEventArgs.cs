using QuanLib.Core.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Events
{
    public readonly struct TickStageEventArgs(int tick, SystemStage stage) : IValueEventArgs
    {
        public readonly int Tick = tick;

        public readonly SystemStage Stage = stage;
    }
}
