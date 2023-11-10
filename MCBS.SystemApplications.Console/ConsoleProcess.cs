using log4net.Core;
using MCBS.Logging;
using QuanLib.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.SystemApplications.Console
{
    public class ConsoleProcess : UnmanagedRunnable
    {
        public ConsoleProcess() : this(ProcessRun.Cmd) { }

        public ConsoleProcess(ProcessRun processRun) : base(LogUtil.GetLogger)
        {
            if (processRun is null)
                throw new ArgumentNullException(nameof(processRun));

            Process = new()
            {
                StartInfo = new(processRun.ExecutableProgram, processRun.StartupArguments)
                {
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    WorkingDirectory = processRun.WorkingDirectory
                }
            };

            _output = new();
        }

        private readonly StringBuilder _output;

        public Process Process { get; }

        public string GetOutput()
        {
            lock (_output)
            {
                string output = _output.ToString();
                _output.Clear();
                return output;
            }
        }

        protected override void Run()
        {
            Process.Start();
            while (IsRunning)
            {
                int c = Process.StandardOutput.Read();
                if (c == -1)
                {
                    IsRunning = false;
                    break;
                }

                lock (_output)
                    _output.Append((char)c);
            }
        }

        protected override void DisposeUnmanaged()
        {
            Process.Kill();
            Process.WaitForExit();
            Process.Dispose();
        }
    }
}
