using QuanLib.Commands;
using QuanLib.Commands.CommandLine;
using QuanLib.Commands.Parsing;
using QuanLib.Commands.Syntaxes;
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
            _consoleCommandReader = new(CommandManager, SyntaxSplitService.Default, SyntaxUnescapeService.Default, NullParser.Default, SingleQuoteParser.Default);
        }

        private readonly ConsoleCommandReader _consoleCommandReader;

        protected override CommandReadResult ReadCommand()
        {
            _consoleCommandReader.Start();
            return _consoleCommandReader.WaitForCompletion();
        }

        public override void Stop()
        {
            _consoleCommandReader.Stop();
            base.Stop();
        }
    }
}
