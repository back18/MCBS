using QuanLib.Core;
using QuanLib.Core.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS
{
    public class TimeAnalysis
    {
        public TimeAnalysis()
        {
            LatestTime = TimeSpan.Zero;
            Ticks20Timer = new(20);
            Ticks1200Timer = new(1200);
            Ticks6000Timer = new(6000);

            Added += OnAdded;
        }

        public TimeSpan LatestTime { get; private set; }

        public QueueTimer Ticks20Timer { get; }

        public QueueTimer Ticks1200Timer { get; }

        public QueueTimer Ticks6000Timer { get; }

        public event EventHandler<TimeAnalysis, TimeSpanEventArgs> Added;

        protected virtual void OnAdded(TimeAnalysis sender, TimeSpanEventArgs args) { }

        public void Append(TimeSpan time)
        {
            Ticks20Timer.Append(time);
            Ticks1200Timer.Append(time);
            Ticks6000Timer.Append(time);
            Added.Invoke(this, new(time));
        }

        public TimeSpan GetAverageTime(Ticks ticks)
        {
            return ticks switch
            {
                Ticks.Ticks1 => LatestTime,
                Ticks.Ticks20 => Ticks20Timer.AverageTime,
                Ticks.Ticks1200 => Ticks1200Timer.AverageTime,
                Ticks.Ticks6000 => Ticks6000Timer.AverageTime,
                _ => throw new InvalidOperationException(),
            };
        }

        public enum Ticks
        {
            Ticks1,

            Ticks20,

            Ticks1200,

            Ticks6000
        }
    }
}
