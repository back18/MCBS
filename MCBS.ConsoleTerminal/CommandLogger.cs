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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MCBS.ConsoleTerminal
{
    public class CommandLogger : UnmanagedRunnable
    {
        public CommandLogger() : base(LogUtil.GetLogger)
        {
            IsWriteToConsole = false;
            IsWriteToFile = false;

            string file = SR.McbsDirectory.LogsDir.Combine("Command.log");
            if (File.Exists(file))
                File.Delete(file);
            _stream = new(file, FileMode.Create, FileAccess.Write, FileShare.Read);
            _writer = new(_stream);
            _queue = new();
        }

        private readonly FileStream _stream;

        private readonly StreamWriter _writer;

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
            _writer.Dispose();
            _stream.Dispose();
        }

        public void Submit(CommandLog commandLog)
        {
            if (commandLog is null)
                throw new ArgumentNullException(nameof(commandLog));

            _queue.Enqueue(commandLog);
        }

        private void Handle(CommandLog commandLog)
        {
            if (commandLog is null)
                throw new ArgumentNullException(nameof(commandLog));

            double timeSpan = (commandLog.Info.ReceivingTime.Ticks - commandLog.Info.SendingTime.Ticks) / 10.0;
            WriteLine($"[Tick={commandLog.Tick}] [Stage={commandLog.Stage}] [Thread={commandLog.ThreadName}] [TimeSpan={timeSpan} us]");
            WriteLine($"[{commandLog.Info.SendingTime:HH:mm:ss:ffffff}] [Send]: {commandLog.Info.Input}");
            WriteLine($"[{commandLog.Info.ReceivingTime:HH:mm:ss:ffffff}] [Receiv]: {commandLog.Info.Output}");
            WriteLine();
        }

        private void Write(string text)
        {
            if (IsWriteToConsole)
                Console.Write(text);
            if (IsWriteToFile)
                _writer.Write(text);
        }

        private void WriteLine()
        {
            if (IsWriteToConsole)
                Console.WriteLine();
            if (IsWriteToFile)
                _writer.WriteLine();
        }

        private void WriteLine(string text)
        {
            if (IsWriteToConsole)
                Console.WriteLine(text);
            if (IsWriteToFile)
                _writer.WriteLine(text);
        }
    }
}
