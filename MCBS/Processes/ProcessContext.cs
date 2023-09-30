using log4net.Core;
using MCBS.Application;
using MCBS.Logging;
using MCBS.State;
using MCBS.UI;
using QuanLib.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Processes
{
    public class ProcessContext : RunnableBase
    {
        private static readonly LogImpl LOGGER = LogUtil.GetLogger();

        internal ProcessContext(ApplicationInfo appInfo, string[] args, IForm? initiator = null) : base(LogUtil.GetLogger)
        {
            if (args is null)
                throw new ArgumentNullException(nameof(args));

            _args = args;
            ApplicationInfo = appInfo ?? throw new ArgumentNullException(nameof(appInfo));
            Application = ApplicationInfo.CreateApplicationInstance();
            Initiator = initiator;
            ID = -1;
            StateManager = new(ProcessState.Unstarted, new StateContext<ProcessState>[]
            {
                new(ProcessState.Unstarted, Array.Empty<ProcessState>(), HandleUnstartedState),
                new(ProcessState.Active, new ProcessState[] { ProcessState.Unstarted }, HandleActiveState),
                new(ProcessState.Stopped, new ProcessState[] { ProcessState.Active }, HandleStoppedState)
            });
        }

        private readonly string[] _args;

        public int ID { get; internal set; }

        public StateManager<ProcessState> StateManager { get; }

        public ProcessState ProcessState => StateManager.CurrentState;

        public ApplicationInfo ApplicationInfo { get; }

        public ApplicationBase Application { get; }

        public IForm? Initiator { get; }

        public string MainThreadName => $"{ApplicationInfo.ID} AppThread #{ID}";

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

        protected virtual bool HandleUnstartedState(ProcessState current, ProcessState next)
        {
            return false;
        }

        protected virtual bool HandleActiveState(ProcessState current, ProcessState next)
        {
            Start($"{ApplicationInfo.ID} AppThread #{ID}");
            return true;
        }

        protected virtual bool HandleStoppedState(ProcessState current, ProcessState next)
        {
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
            return true;
        }

        public void Handle()
        {
            StateManager.HandleAllState();
        }

        protected override void Run()
        {
            LOGGER.Info($"进程“{ApplicationInfo.ID} #{ID}”已开始运行");
            object? result = Application.Main(_args);
            LOGGER.Info($"进程“{ApplicationInfo.ID} #{ID}”停止开始运行，返回值为 {result ?? "null"}");
        }

        public ProcessContext StartProcess()
        {
            StateManager.AddNextState(ProcessState.Active);
            return this;
        }

        public void StopProcess()
        {
            StateManager.AddNextState(ProcessState.Stopped);
        }

        public override string ToString()
        {
            return $"State={ProcessState}, PID={ID}, AppID={ApplicationInfo.ID}";
        }
    }
}
