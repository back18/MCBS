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
            string[] lines = Enum.GetNames<TStage>();
            TStage[] stages = Enum.GetValues<TStage>();
            int maxWidth = stages.Select(s => s.ToString()).Max(CharacterWidthMapping.Instance.GetWidth) + 4;

            foreach (TStage stage in stages)
            {
                string name = stage.ToString();
                stringBuilder.Append(name);
                stringBuilder.Append('-', maxWidth - CharacterWidthMapping.Instance.GetWidth(name));
                stringBuilder.Append(Math.Round(source.StageTimes[stage].GetAverageTime(Ticks.Ticks20).TotalMilliseconds, 3));
                stringBuilder.Append("ms");
                stringBuilder.Append('\n');
            }

            stringBuilder.AppendFormat("MSPT: {0}ms\nTPS: {1}", Math.Round(source.TickTime.GetAverageTime(Ticks.Ticks20).TotalMilliseconds, 3), Math.Max(50, Math.Round(1000 / source.TickTime.GetAverageTime(Ticks.Ticks20).TotalMilliseconds, 3)));

            return stringBuilder.ToString();
        }
    }
}
