using MCBS.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Analyzer
{
    public class SystemStageRecord : MsptRecord<SystemStage>
    {
        public SystemStageRecord(MinecraftBlockScreen owner) : base(owner)
        {
            owner.SystemStageSatrt += MCBS_SystemStageSatrt;
            owner.SystemStageEnd += MCBS_SystemStageEnd;
        }

        private void MCBS_SystemStageSatrt(MinecraftBlockScreen sender, TickStageEventArgs e)
        {
            StageStopwatchs[e.Stage].Restart();
        }

        private void MCBS_SystemStageEnd(MinecraftBlockScreen sender, TickStageEventArgs e)
        {
            StageStopwatchs[e.Stage].Stop();
        }
    }
}
