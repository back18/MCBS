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
    public class SimpleTerminal : Terminal
    {
        public SimpleTerminal(CommandSender commandSender, ILoggerProvider? loggerProvider = null) : base(commandSender, loggerProvider)
        {
            SyntaxInterpreter = new(CommandManager, SyntaxSplitService.Default, SyntaxUnescapeService.Default, NullParser.Default, SingleQuoteParser.Default);
        }

        public SyntaxInterpreter SyntaxInterpreter { get; }

        protected override CommandReadResult ReadCommand()
        {
            string? input = Console.ReadLine();
            if (input is null)
                return CommandReadResult.Empty;

            SyntaxTree syntaxTree = SyntaxInterpreter.BuildSyntaxTree(input);
            string identifier = syntaxTree.BuildIdentifier();
            CommandManager.TryGetValue(identifier, out var command);

            return new CommandReadResult(command, syntaxTree);
        }
    }
}
