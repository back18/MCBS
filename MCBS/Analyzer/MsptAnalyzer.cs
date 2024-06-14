using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Analyzer
{
    public class MsptAnalyzer<TStage> where TStage : struct, Enum
    {
        public MsptAnalyzer()
        {
            _stages = Enum.GetValues<TStage>();

            Dictionary<TStage, MsptSlice> stageTimes = [];
            foreach (TStage stage in _stages)
                stageTimes.Add(stage, new());

            StageTimes = stageTimes.AsReadOnly();
            TickTime = new();
        }

        private readonly TStage[] _stages;

        public ReadOnlyDictionary<TStage, MsptSlice> StageTimes { get; }

        public MsptSlice TickTime { get; }

        internal void Update(MsptRecord<TStage> msptRecord)
        {
            ArgumentNullException.ThrowIfNull(msptRecord, nameof(msptRecord));

            TimeSpan total = TimeSpan.Zero;

            foreach (TStage stage in _stages)
            {
                TimeSpan time = msptRecord.GetTime(stage);
                total += time;
                StageTimes[stage].Update(time);
            }

            TickTime.Update(total);
        }
    }
}
