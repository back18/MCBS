using QuanLib.Minecraft.Command;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.ConsoleTerminal
{
    public readonly struct CommandLog(CommandInfo commandInfo, int gameTick, int systemTick, SystemStage systemStage)
    {
        public readonly CommandInfo CommandInfo = commandInfo;

        public readonly int GameTick = gameTick;

        public readonly int SystemTick = systemTick;

        public readonly SystemStage SystemStage = systemStage;
    }
}
