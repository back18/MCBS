using log4net.Core;
using MCBS.Application;
using MCBS.Forms;
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
    public class ProcessContext : RunnableBase, ITickable
    {
        private static readonly LogImpl LOGGER = LogUtil.GetLogger();

        internal ProcessContext(ApplicationManifest applicationManifest, string[] args, IForm? initiator = null) : base(Logbuilder.Default)
        {
            ArgumentNullException.ThrowIfNull(applicationManifest, nameof(applicationManifest));
            ArgumentNullException.ThrowIfNull(args, nameof(args));

            _args = args;
            Application = applicationManifest;
            Program = Application.CreateApplicationInstance();
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

        public ApplicationManifest Application { get; }

        public IProgram Program { get; }

        public IForm? Initiator { get; }

        protected override void OnStarted(IRunnable sender, EventArgs e)
        {
            base.OnStarted(sender, e);

            if (Thread is not null)
                Thread.IsBackground = true;
        }

        protected override void OnStopped(IRunnable sender, EventArgs e)
        {
            base.OnStopped(sender, e);

            foreach (var form in Program.GetForms())
                form.CloseForm();
        }

        protected virtual bool HandleUnstartedState(ProcessState current, ProcessState next)
        {
            return false;
        }

        protected virtual bool HandleActiveState(ProcessState current, ProcessState next)
        {
            Start($"{Application.ID} AppThread #{ID}");
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

        public void OnTick()
        {
            StateManager.HandleAllState();
        }

        protected override void Run()
        {
            string args = _args.Length == 0 ? "empty" : string.Join(", ", _args.Select(arg => $"\"{arg}\""));
            FormContext ? formContext = Initiator is null ? null : MCOS.Instance.FormContextOf(Initiator);
            if (formContext is null)
                LOGGER.Info($"进程({Application.ID} #{ID})已启动，启动参数为 {args}");
            else
                LOGGER.Info($"进程({Application.ID} #{ID})已被窗体({formContext.Form.Text} #{formContext.ID})启动，启动参数为 {args}");
            object? result = Program.Main(_args);
            LOGGER.Info($"进程({Application.ID} #{ID})已停止，返回值为 {result ?? "null"}");
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
            return $"State={ProcessState}, PID={ID}, AppID={Application.ID}";
        }
    }
}
