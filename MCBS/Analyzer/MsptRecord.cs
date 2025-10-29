using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Analyzer
{
    public class MsptRecord<TStage> where TStage : struct, Enum
    {
        public MsptRecord(MinecraftBlockScreen owner)
        {
            ArgumentNullException.ThrowIfNull(owner, nameof(owner));

            Stages = Enum.GetValues<TStage>();
            TickStopwatch = new Stopwatch();
            StageStopwatchs = Stages.ToDictionary(item => item, item => new Stopwatch()).AsReadOnly();

            owner.TickStart += MCBS_TickStart;
            owner.TickEnd += MCBS_TickEnd;
        }

        protected readonly TStage[] Stages;

        protected readonly Stopwatch TickStopwatch;

        protected readonly ReadOnlyDictionary<TStage, Stopwatch> StageStopwatchs;

        public TimeSpan GetStageTime(TStage stage)
        {
            return StageStopwatchs[stage].Elapsed;
        }

        public TimeSpan GetTickTime()
        {
            return TickStopwatch.Elapsed;
        }

        public void Reset()
        {
            TickStopwatch.Reset();
            foreach (TStage stage in Stages)
                StageStopwatchs[stage].Reset();
        }

        public void Stop()
        {
            TickStopwatch.Stop();
            foreach (TStage stage in Stages)
                StageStopwatchs[stage].Stop();
        }

        private void MCBS_TickStart(int tick)
        {
            TickStopwatch.Restart();
        }

        private void MCBS_TickEnd(int tick)
        {
            TickStopwatch.Stop();
        }
    }
}
