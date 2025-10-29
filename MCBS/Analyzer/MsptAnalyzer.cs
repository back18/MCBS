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

            TickTime = new MsptSlice();
            StageTimes = _stages.ToDictionary(item => item, item => new MsptSlice()).AsReadOnly();
        }

        private readonly TStage[] _stages;

        public MsptSlice TickTime { get; }

        public ReadOnlyDictionary<TStage, MsptSlice> StageTimes { get; }

        internal void Update(MsptRecord<TStage> msptRecord)
        {
            ArgumentNullException.ThrowIfNull(msptRecord, nameof(msptRecord));

            TickTime.Update(msptRecord.GetTickTime());
            foreach (TStage stage in _stages)
            {
                TimeSpan time = msptRecord.GetStageTime(stage);
                StageTimes[stage].Update(time);
            }
        }
    }
}
