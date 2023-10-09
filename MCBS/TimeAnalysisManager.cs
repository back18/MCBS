using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS
{
    public class TimeAnalysisManager : IReadOnlyDictionary<string, TimeAnalysis>
    {
        public TimeAnalysisManager()
        {
            TickTimer = new();
            _items = new()
            {
                { Key.ScreenScheduling, new() },
                { Key.ProcessScheduling, new() },
                { Key.FormScheduling, new() },
                { Key.InteractionScheduling, new() },
                { Key.RightClickObjectiveScheduling, new() },
                { Key.ScreenBuildScheduling, new() },
                { Key.HandleScreenInput, new() },
                { Key.HandleBeforeFrame, new() },
                { Key.HandleUIRendering, new() },
                { Key.HandleScreenOutput, new() },
                { Key.HandleAfterFrame, new() },
                { Key.HandleSystemInterrupt, new() }
            };
        }

        private readonly Dictionary<string, TimeAnalysis> _items;

        public TimeAnalysis this[string key] => _items[key];

        public IEnumerable<string> Keys => _items.Keys;

        public IEnumerable<TimeAnalysis> Values => _items.Values;

        public int Count => _items.Count;

        public TimeAnalysis TickTimer { get; }

        public void Submit(IDictionary<string, TimeSpan> times)
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
            TickTimer.Append(total);
        }

        public bool ContainsKey(string key)
        {
            return _items.ContainsKey(key);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out TimeAnalysis value)
        {
            return _items.TryGetValue(key, out value);
        }

        public IEnumerator<KeyValuePair<string, TimeAnalysis>> GetEnumerator()
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
            foreach (string key in _items.Keys)
            {
                if (key.Length > maxLength)
                    maxLength = key.Length;
            }

            maxLength += 3;
            StringBuilder sb = new();
            foreach (var item in _items)
                sb.AppendLine(FromKey(item.Key) + FromTime(item.Value.GetAverageTime(ticks)));
            sb.AppendLine($"{_items.Count} timers in total, with a total time of {FromTime(TickTimer.GetAverageTime(ticks))}");
            return sb.ToString().TrimEnd();

            string FromKey(string key)
            {
                return key.PadRight(maxLength, '-');
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
