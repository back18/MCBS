using log4net.Core;
using MCBS.Logging;
using MCBS.UI;
using QuanLib.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace MCBS
{
    public class Process : RunnableBase
    {
        private static readonly LogImpl LOGGER = LogUtil.GetLogger();

        internal Process(ApplicationInfo appInfo, string[] args, IForm? initiator = null) : base(LogUtil.GetLogger)
        {
            if (args is null)
                throw new ArgumentNullException(nameof(args));

            _args = args;
            ApplicationInfo = appInfo ?? throw new ArgumentNullException(nameof(appInfo));
            Application = ApplicationInfo.CreateApplicationInstance();
            Initiator = initiator;
            ID = -1;
        }

        private readonly string[] _args;

        public int ID { get; internal set; }

        public ApplicationInfo ApplicationInfo { get; }

        public Application Application { get; }

        public IForm? Initiator { get; }

        public string MainThreadName => $"{ApplicationInfo.ID} AppThread #{ID}";

        public ProcessState ProcessState { get; private set; }

        protected override void OnStarted(IRunnable sender, EventArgs e)
        {
            base.OnStarted(sender, e);

            if (Thread is not null)
                Thread.IsBackground = true;
        }

        protected override void OnStopped(IRunnable sender, EventArgs e)
        {
            base.OnStopped(sender, e);

            foreach (var form in Application.GetForms())
                form.CloseForm();
        }

        protected override void Run()
        {
            LOGGER.Info($"进程“{MainThreadName}”已开始运行");
            object? result = Application.Main(_args);
            LOGGER.Info($"进程“{MainThreadName}”停止开始运行，返回值为 {result ?? "null"}");
        }

        public void Handle()
        {
            switch (ProcessState)
            {
                case ProcessState.Unstarted:
                    break;
                case ProcessState.Starting:
                    Start(MainThreadName);
                    ProcessState = ProcessState.Running;
                    break;
                case ProcessState.Running:
                    break;
                case ProcessState.Stopped:
                    if (Thread is not null && Thread.IsAlive)
                    {
                        try
                        {
                            Thread.Abort();
                        }
                        catch
                        {

                        }
                    }
                    Stop();
                    break;
                default:
                    break;
            }
        }

        public Process StartProcess()
        {
            ProcessState = ProcessState.Starting;
            return this;
        }

        public void StopProcess()
        {
            ProcessState = ProcessState.Stopped;
        }

        public override string ToString()
        {
            return $"State={ProcessState}, PID={ID}, AppID={ApplicationInfo.ID}";
        }
    }
}
