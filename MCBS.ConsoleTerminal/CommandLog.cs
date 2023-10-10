using QuanLib.Minecraft.Command;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.ConsoleTerminal
{
    public class CommandLog
    {
        public CommandLog(CommandInfo info, int tick, Stage stage, string threadName)
        {
            if (info is null)
                throw new ArgumentNullException(nameof(info));
            if (string.IsNullOrEmpty(threadName))
                throw new ArgumentException($"“{nameof(threadName)}”不能为 null 或空。", nameof(threadName));

            Info = info;
            Tick = tick;
            Stage = stage;
            ThreadName = threadName;
        }

        public CommandInfo Info { get; }

        public int Tick { get; }

        public Stage Stage { get; }

        public string ThreadName { get; }
    }
}
