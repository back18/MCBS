using QuanLib.Commands;
using QuanLib.Commands.CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.ConsoleTerminal
{
    public class SimpleTerminal : Terminal
    {
        public SimpleTerminal(CommandSender commandSender) : base(commandSender)
        {
            CommandParser = new(CommandManager);
        }

        public CommandParser CommandParser { get; }

        protected override CommandReaderResult ReadCommand()
        {
            string? input = Console.ReadLine();
            WordCollection wordCollection = CommandParser.Parse(input ?? string.Empty);
            string identifier = wordCollection.GetIdentifier();
            CommandManager.TryGetValue(identifier, out var command);
            string[] args = wordCollection.GetArgumentTexts();
            return new(command, args);
        }
    }
}
