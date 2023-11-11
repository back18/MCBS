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
        public ConsoleProcess() : this(ProcessInfo.CMD) { }

        public ConsoleProcess(ProcessInfo processInfo) : base(LogUtil.GetLogger)
        {
            if (processInfo is null)
                throw new ArgumentNullException(nameof(processInfo));

            Process = new()
            {
                StartInfo = new(processInfo.ExecutableProgram, processInfo.StartupArguments)
                {
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    WorkingDirectory = processInfo.WorkingDirectory
                }
            };
        }

        private StandardStreamReader? _outputReader;

        private StandardStreamReader? _errorReader;

        public Process Process { get; }

        public string GetOutputText()
        {
            return _outputReader?.GetOutput() ?? string.Empty;
        }

        public string GetErrorText()
        {
            return _errorReader?.GetOutput() ?? string.Empty;
        }

        protected override void Run()
        {
            Process.Start();
            _outputReader = new(Process.StandardOutput);
            _errorReader = new(Process.StandardError);
            _outputReader.Start("StandardOutputReader Thread");
            _errorReader.Start("StandardErrorReader Thread");

            Task outputTask = _outputReader.WaitForStopAsync();
            Task errorTask = _errorReader.WaitForStopAsync();
            Task.WaitAll(outputTask, errorTask);
        }

        protected override void DisposeUnmanaged()
        {
            Process.Kill();
            Process.WaitForExit();
            Process.Dispose();
        }
    }
}
