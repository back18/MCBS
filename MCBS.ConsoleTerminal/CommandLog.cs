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
        public CommandLog(CommandInfo info, int gameTick, int systemTick, SystemStage systemStage, string threadName)
        {
            ArgumentNullException.ThrowIfNull(info, nameof(info));
            ArgumentException.ThrowIfNullOrEmpty(threadName, nameof(threadName));

            Info = info;
            GameTick = gameTick;
            SystemTick = systemTick;
            SystemStage = systemStage;
            ThreadName = threadName;
        }

        public CommandInfo Info { get; }

        public int GameTick { get; private set; }

        public int SystemTick { get; private set; }

        public SystemStage SystemStage { get; private set; }

        public string ThreadName { get; }
    }
}
