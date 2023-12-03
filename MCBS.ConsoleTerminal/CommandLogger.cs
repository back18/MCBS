using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using MCBS.Logging;
using QuanLib.Core;
using QuanLib.Minecraft.Command;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MCBS.ConsoleTerminal
{
    public class CommandLogger : UnmanagedRunnable
    {
        public CommandLogger() : base(Logbuilder.Default)
        {
            string file = SR.McbsDirectory.LogsDir.Combine("Command.log");
            if (File.Exists(file))
                File.Delete(file);

            IsWriteToConsole = false;
            IsWriteToFile = false;

            _memoryStream = new();
            _fileStream = new(file, FileMode.Create, FileAccess.Write, FileShare.Read);

            _memoryWriter = new StreamWriter(_memoryStream, Encoding.UTF8);
            _fileWriter = new StreamWriter(_fileStream, Encoding.UTF8);
            _consoleWriter = Console.Out;

            _queue = new();
        }

        private readonly MemoryStream _memoryStream;

        private readonly FileStream _fileStream;

        private readonly TextWriter _consoleWriter;

        private readonly TextWriter _fileWriter;

        private readonly TextWriter _memoryWriter;

        private readonly ConcurrentQueue<CommandLog> _queue;

        public bool IsWriteToConsole { get; set; }

        public bool IsWriteToFile { get; set; }

        protected override void Run()
        {
            while (IsRunning)
            {
                if (!_queue.TryDequeue(out var commandLog))
                {
                    Thread.Sleep(10);
                    continue;
                }

                Handle(commandLog);
            }
        }

        protected override void DisposeUnmanaged()
        {
            _memoryWriter.Dispose();
            _memoryStream.Dispose();
            _fileWriter.Dispose();
            _fileStream.Dispose();
        }

        public void Submit(CommandLog commandLog)
        {
            ArgumentNullException.ThrowIfNull(commandLog, nameof(commandLog));

            _queue.Enqueue(commandLog);
        }

        private void Handle(CommandLog commandLog)
        {
            ArgumentNullException.ThrowIfNull(commandLog, nameof(commandLog));

            double timeSpan = (commandLog.Info.ReceivingTime.Ticks - commandLog.Info.SendingTime.Ticks) / 10.0;
            WriteLine($"[GameTick={commandLog.GameTick}] [SystemTick={commandLog.SystemTick}] [Stage={commandLog.SystemStage}] [Thread={commandLog.ThreadName}] [TimeSpan={timeSpan} us]");
            WriteLine($"[{commandLog.Info.SendingTime:HH:mm:ss:ffffff}] [Send]: {commandLog.Info.Input}");
            WriteLine($"[{commandLog.Info.ReceivingTime:HH:mm:ss:ffffff}] [Receiv]: {commandLog.Info.Output}");
            WriteLine();
        }

        private void Write(string text)
        {
            _memoryWriter.Write(text);
            if (IsWriteToConsole)
                _consoleWriter.Write(text);
            if (IsWriteToFile)
                _fileWriter.Write(text);
        }

        private void WriteLine()
        {
            _memoryWriter.WriteLine();
            if (IsWriteToConsole)
                _consoleWriter.WriteLine();
            if (IsWriteToFile)
                _fileWriter.WriteLine();
        }

        private void WriteLine(string text)
        {
            _memoryWriter.WriteLine(text);
            if (IsWriteToConsole)
                _consoleWriter.WriteLine(text);
            if (IsWriteToFile)
                _fileWriter.WriteLine(text);
        }

        public byte[] GetBuffer()
        {
            return _memoryStream.ToArray();
        }
    }
}
