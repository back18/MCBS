using QuanLib.Core;
using QuanLib.Core.Events;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Analyzer
{
    public class MsptSlice
    {
        public MsptSlice()
        {
            LatestTime = TimeSpan.Zero;
            Times20Ticks = new(20);
            Times1200Ticks = new(1200);
            Times6000Ticks = new(6000);

            TimeUpdated += OnTimeUpdated;
        }

        public TimeSpan LatestTime { get; private set; }

        public QueueTimer Times20Ticks { get; }

        public QueueTimer Times1200Ticks { get; }

        public QueueTimer Times6000Ticks { get; }

        public event EventHandler<MsptSlice, EventArgs<TimeSpan>> TimeUpdated;

        protected virtual void OnTimeUpdated(MsptSlice sender, EventArgs<TimeSpan> args) { }

        internal void Update(TimeSpan time)
        {
            Times20Ticks.Update(time);
            Times1200Ticks.Update(time);
            Times6000Ticks.Update(time);
            TimeUpdated.Invoke(this, new(time));
        }

        public TimeSpan GetAverageTime(Ticks ticks)
        {
            return ticks switch
            {
                Ticks.Ticks1 => LatestTime,
                Ticks.Ticks20 => Times20Ticks.AverageTime,
                Ticks.Ticks1200 => Times1200Ticks.AverageTime,
                Ticks.Ticks6000 => Times6000Ticks.AverageTime,
                _ => throw new InvalidEnumArgumentException()
            };
        }
    }
}
