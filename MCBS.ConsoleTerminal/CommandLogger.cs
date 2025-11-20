using QuanLib.Core;
using QuanLib.Logging;
using QuanLib.Minecraft.Command;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.ConsoleTerminal
{
    public class CommandLogger : UnmanagedRunnable
    {
        public CommandLogger(string logFilePath, int maxCacheCount = 10000, bool deduplication = true) : base(Log4NetManager.Instance.GetProvider())
        {
            ArgumentException.ThrowIfNullOrEmpty(logFilePath, nameof(logFilePath));
            ThrowHelper.ArgumentOutOfMin(1, maxCacheCount, nameof(maxCacheCount));

            MaxCacheCount = maxCacheCount;
            Deduplication = deduplication;
            IsWriteToConsole = false;
            IsWriteToFile = false;

            _flushed = true;
            _fileStream = new(logFilePath, FileMode.Create, FileAccess.Write, FileShare.Read);
            _fileWriter = new StreamWriter(_fileStream, Encoding.UTF8);

            _queue = new();
            _linkedlist = new();
            _blacklist = [];
        }

        private bool _flushed;

        private readonly FileStream _fileStream;

        private readonly TextWriter _fileWriter;

        private readonly ConcurrentQueue<CommandLog> _queue;

        private readonly LinkedList<CommandLog> _linkedlist;

        private readonly List<string> _blacklist;

        public int MaxCacheCount { get; }

        public bool Deduplication { get; }

        public bool IsWriteToConsole { get; set; }

        public bool IsWriteToFile { get; set; }

        protected override void Run()
        {
            while (IsRunning)
            {
                if (!_queue.TryDequeue(out var commandLog))
                {
                    if (!_flushed)
                        Flush();
                    Thread.Sleep(10);
                    continue;
                }

                if (IsWriteToConsole || IsWriteToFile)
                    WriteLog(commandLog);

                _linkedlist.AddLast(commandLog);
                while (_linkedlist.Count > MaxCacheCount)
                    _linkedlist.RemoveFirst();
            }
        }

        protected override void DisposeUnmanaged()
        {
            _fileWriter.Dispose();
            _fileStream.Dispose();
        }

        public void Submit(CommandLog commandLog)
        {
            ArgumentNullException.ThrowIfNull(commandLog, nameof(commandLog));

            _queue.Enqueue(commandLog);
        }

        public string[] GetLlacklist()
        {
            return _blacklist.ToArray();
        }

        public void AddBlacklist(string command)
        {
            ArgumentNullException.ThrowIfNull(command, nameof(command));

            _blacklist.Add(command);
        }

        public bool RemoveBlacklist(string command)
        {
            ArgumentNullException.ThrowIfNull(command, nameof(command));

            return _blacklist.Remove(command);
        }

        private void WriteLog(CommandLog commandLog)
        {
            string input = commandLog.CommandInfo.Input;
            if (_blacklist.Contains(input))
                return;

            if (Deduplication)
            {
                string output = commandLog.CommandInfo.Output;
                LinkedListNode<CommandLog>? linkedlistNode = _linkedlist.Last;

                for (int i = 0; i < 100; i++)
                {
                    if (linkedlistNode is null)
                        break;

                    CommandInfo commandInfo = linkedlistNode.Value.CommandInfo;
                    if (commandInfo.Input == input)
                    {
                        if (commandInfo.Output == output)
                            return;
                        else
                            break;
                    }

                    linkedlistNode = linkedlistNode.Previous;
                }
            }

            StringBuilder stringBuilder = ToLog(commandLog);
            Write(stringBuilder);
        }

        private void Write(string? text)
        {
            if (IsWriteToConsole)
                Console.Write(text);
            if (IsWriteToFile)
                _fileWriter.Write(text);

            _flushed = false;
        }

        private void Write(StringBuilder? text)
        {
            if (IsWriteToConsole)
                Console.Write(text);
            if (IsWriteToFile)
                _fileWriter.Write(text);

            _flushed = false;
        }

        private void WriteLine(string? text)
        {
            if (IsWriteToConsole)
                Console.WriteLine(text);
            if (IsWriteToFile)
                _fileWriter.WriteLine(text);

            _flushed = false;
        }

        private void WriteLine(StringBuilder? text)
        {
            if (IsWriteToConsole)
                Console.WriteLine(text);
            if (IsWriteToFile)
                _fileWriter.WriteLine(text);

            _flushed = false;
        }

        private void WriteLine()
        {
            if (IsWriteToConsole)
                Console.WriteLine();
            if (IsWriteToFile)
                _fileWriter.WriteLine();

            _flushed = false;
        }

        public void Flush()
        {
            _fileWriter.Flush();
            _flushed = true;
        }

        public void Dump(Stream outputStream)
        {
            ArgumentNullException.ThrowIfNull(outputStream, nameof(outputStream));
            if (!outputStream.CanWrite)
                throw new NotSupportedException("Stream must be writable");

            if (_linkedlist.Count == 0)
                return;

            using TextWriter textWriter = new StreamWriter(outputStream, Encoding.UTF8);
            CommandLog[] commandLogs = _linkedlist.ToArray();

            foreach (CommandLog commandLog in commandLogs)
                textWriter.Write(ToLog(commandLog));
        }

        public void Dump(Stream outputStream, int maxLogCount)
        {
            ThrowHelper.ArgumentOutOfMin(1, maxLogCount, nameof(maxLogCount));
            ArgumentNullException.ThrowIfNull(outputStream, nameof(outputStream));
            if (!outputStream.CanWrite)
                throw new NotSupportedException("Stream must be writable");

            if (_linkedlist.Count == 0)
                return;

            using TextWriter textWriter = new StreamWriter(outputStream, Encoding.UTF8);
            int lenght = Math.Min(maxLogCount, _linkedlist.Count);
            List<CommandLog> commandLogs = new(lenght);
            LinkedListNode<CommandLog>? linkedlistNode = _linkedlist.Last;

            for (int i = 0; i < lenght; i++)
            {
                if (linkedlistNode is null)
                    break;

                commandLogs.Add(linkedlistNode.Value);
                linkedlistNode = linkedlistNode.Previous;
            }

            for (int i = commandLogs.Count - 1; i >= 0; i--)
                textWriter.Write(ToLog(commandLogs[i]));
        }

        private static StringBuilder ToLog(CommandLog commandLog)
        {
            CommandInfo commandInfo = commandLog.CommandInfo;
            long elapsedTicks = commandInfo.EndTimeStamp - commandInfo.StartTimeStamp;
            long us = elapsedTicks / TimeSpan.TicksPerMicrosecond;
            long ns = elapsedTicks % TimeSpan.TicksPerMicrosecond;

            StringBuilder stringBuilder = new();
            stringBuilder.AppendLine($"[GT={commandLog.GameTick}] [ST={commandLog.SystemTick}] [STAGE={commandLog.SystemStage}] [US={us}.{ns}]");
            stringBuilder.AppendLine($"[{commandInfo.StartTime:HH:mm:ss:ffffff}] [Send]: {commandInfo.Input}");
            stringBuilder.AppendLine($"[{commandInfo.EndTime:HH:mm:ss:ffffff}] [Receiv]: {commandInfo.Output}");
            stringBuilder.AppendLine();

            return stringBuilder;
        }
    }
}
