using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS
{
    public class TimeAnalysisManager : IReadOnlyDictionary<Stage, TimeAnalysis>
    {
        public TimeAnalysisManager()
        {
            TickTimer = new();
            _items = new()
            {
                { Stage.ScreenScheduling, new() },
                { Stage.ProcessScheduling, new() },
                { Stage.FormScheduling, new() },
                { Stage.InteractionScheduling, new() },
                { Stage.RightClickObjectiveScheduling, new() },
                { Stage.ScreenBuildScheduling, new() },
                { Stage.HandleScreenInput, new() },
                { Stage.HandleBeforeFrame, new() },
                { Stage.HandleUIRendering, new() },
                { Stage.HandleScreenOutput, new() },
                { Stage.HandleAfterFrame, new() },
                { Stage.HandleSystemInterrupt, new() },
                { Stage.Other, new() }
            };
        }

        private readonly Dictionary<Stage, TimeAnalysis> _items;

        public TimeAnalysis this[Stage key] => _items[key];

        public IEnumerable<Stage> Keys => _items.Keys;

        public IEnumerable<TimeAnalysis> Values => _items.Values;

        public int Count => _items.Count;

        public TimeAnalysis TickTimer { get; }

        public void Submit(IDictionary<Stage, TimeSpan> times, TimeSpan tickTime)
        {
            TimeSpan total = TimeSpan.Zero;
            foreach (var item in _items)
            {
                if (times.TryGetValue(item.Key, out var time))
                    item.Value.Append(time);
                else
                    item.Value.Append(TimeSpan.Zero);
                total += time;
            }
            TickTimer.Append(tickTime);
            _items[Stage.Other].Append(tickTime - total);
        }

        public bool ContainsKey(Stage key)
        {
            return _items.ContainsKey(key);
        }

        public bool TryGetValue(Stage key, [MaybeNullWhen(false)] out TimeAnalysis value)
        {
            return _items.TryGetValue(key, out value);
        }

        public IEnumerator<KeyValuePair<Stage, TimeAnalysis>> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_items).GetEnumerator();
        }

        public override string ToString()
        {
            return ToString(TimeAnalysis.Ticks.Ticks20);
        }

        public string ToString(TimeAnalysis.Ticks ticks)
        {
            int maxLength = 0;
            foreach (Stage key in _items.Keys)
            {
                int length = key.ToString().Length;
                if (length > maxLength)
                    maxLength = length;
            }

            maxLength += 3;
            StringBuilder sb = new();
            foreach (var item in _items)
                sb.AppendLine(FromKey(item.Key) + FromTime(item.Value.GetAverageTime(ticks)));
            sb.AppendLine($"{_items.Count} timers in total, with a total time of {FromTime(TickTimer.GetAverageTime(ticks))}");
            return sb.ToString().TrimEnd();

            string FromKey(Stage key)
            {
                return key.ToString().PadRight(maxLength, '-');
            }

            static string FromTime(TimeSpan time)
            {
                return Math.Round(time.TotalMilliseconds, 3).ToString() + "ms";
            }
        }

        public class Key
        {
            public const string ScreenScheduling = nameof(ScreenScheduling);

            public const string ProcessScheduling = nameof(ProcessScheduling);

            public const string FormScheduling = nameof(FormScheduling);

            public const string InteractionScheduling = nameof(InteractionScheduling);

            public const string RightClickObjectiveScheduling = nameof(RightClickObjectiveScheduling);

            public const string ScreenBuildScheduling = nameof(ScreenBuildScheduling);

            public const string HandleScreenInput = nameof(HandleScreenInput);

            public const string HandleBeforeFrame = nameof(HandleBeforeFrame);

            public const string HandleUIRendering = nameof(HandleUIRendering);

            public const string HandleScreenOutput = nameof(HandleScreenOutput);

            public const string HandleAfterFrame = nameof(HandleAfterFrame);

            public const string HandleSystemInterrupt = nameof(HandleSystemInterrupt);
        }
    }
}
