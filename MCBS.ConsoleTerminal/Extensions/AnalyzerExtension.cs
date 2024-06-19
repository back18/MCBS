using MCBS.Analyzer;
using QuanLib.Consoles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.ConsoleTerminal.Extensions
{
    public static class AnalyzerExtension
    {
        public static string ToConsoleViwe<TStage>(this MsptAnalyzer<TStage> source) where TStage : struct, Enum
        {
            ArgumentNullException.ThrowIfNull(source, nameof(source));

            StringBuilder stringBuilder = new();
            TStage[] stages = Enum.GetValues<TStage>();
            int maxWidth = stages.Select(s => s.ToString()).Max(CharacterWidthMapping.Instance.GetWidth) + 4;

            foreach (TStage stage in stages)
            {
                string name = stage.ToString();
                stringBuilder.Append(name);
                stringBuilder.Append('-', maxWidth - CharacterWidthMapping.Instance.GetWidth(name));
                stringBuilder.Append(source.StageTimes[stage].GetAverageTime(Ticks.Ticks20).TotalMilliseconds);
                stringBuilder.Append("ms");
                stringBuilder.Append('\n');
            }

            stringBuilder.AppendFormat("MSPT: {0}ms\nTPS: {1}", source.TickTime.GetAverageTime(Ticks.Ticks20).TotalMilliseconds, Math.Min(1000 / source.TickTime.GetAverageTime(Ticks.Ticks20).TotalMilliseconds, 20));

            return stringBuilder.ToString();
        }
    }
}
