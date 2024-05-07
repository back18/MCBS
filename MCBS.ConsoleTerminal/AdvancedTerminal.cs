using QuanLib.Commands;
using QuanLib.Commands.CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.ConsoleTerminal
{
    public class AdvancedTerminal : Terminal
    {
        public AdvancedTerminal(CommandSender commandSender) : base(commandSender)
        {
            _consoleCommandReader = new(CommandManager);
        }

        private readonly ConsoleCommandReader _consoleCommandReader;

        protected override CommandReaderResult ReadCommand()
        {
            _consoleCommandReader.Start();
            _consoleCommandReader.WaitForStop();
            return _consoleCommandReader.GetResult();
        }
    }
}
