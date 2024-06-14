using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Analyzer
{
    public class MsptRecord<TStage> where TStage : struct, Enum
    {
        public MsptRecord()
        {
            _stages = Enum.GetValues<TStage>();
            _stageTimes = [];
            foreach (TStage stage in _stages)
                _stageTimes.Add(stage, TimeSpan.Zero);
        }

        private readonly TStage[] _stages;

        private readonly Dictionary<TStage, TimeSpan> _stageTimes;

        public TimeSpan GetTime(TStage stage)
        {
            return _stageTimes[stage];
        }

        public void Execute(Action action, TStage stage)
        {
            ArgumentNullException.ThrowIfNull(action, nameof(action));

            Stopwatch stopwatch = Stopwatch.StartNew();
            action.Invoke();
            stopwatch.Stop();

            _stageTimes[stage] += stopwatch.Elapsed;
        }

        public void Clear()
        {
            foreach (TStage stage in _stages)
                _stageTimes[stage] = TimeSpan.Zero;
        }
    }
}
