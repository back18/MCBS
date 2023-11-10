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
            _outputReader.Start();
            _errorReader.Start();

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
