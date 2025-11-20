using QuanLib.Commands;
using QuanLib.Commands.CommandLine;
using QuanLib.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.ConsoleTerminal
{
    public class AdvancedTerminal : Terminal
    {
        public AdvancedTerminal(CommandSender commandSender, ILoggerProvider? loggerProvider = null) : base(commandSender, loggerProvider)
        {
            _consoleCommandReader = new(CommandManager);
        }

        private readonly ConsoleCommandReader _consoleCommandReader;

        protected override CommandReaderResult? ReadCommand()
        {
            _consoleCommandReader.Start();
            _consoleCommandReader.WaitForStop();

            try
            {
                return _consoleCommandReader.GetResult();
            }
            catch
            {
                return null;
            }
        }

        public override void Stop()
        {
            _consoleCommandReader.Stop();
            base.Stop();
        }
    }
}
