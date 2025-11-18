using FFmpeg.AutoGen;
using log4net.Core;
using MCBS.Application;
using MCBS.Forms;
using MCBS.UI;
using QuanLib.Core;
using QuanLib.Logging;
using QuanLib.TickLoop;
using QuanLib.TickLoop.StateMachine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Processes
{
    public class ProcessContext : RunnableBase, ITickUpdatable
    {
        private static readonly LogImpl LOGGER = LogManager.Instance.GetLogger();

        internal ProcessContext(ApplicationManifest applicationManifest, string[] args, IForm? initiator = null) : base(LogManager.Instance.LoggerGetter)
        {
            ArgumentNullException.ThrowIfNull(applicationManifest, nameof(applicationManifest));
            ArgumentNullException.ThrowIfNull(args, nameof(args));

            _args = args;
            Application = applicationManifest;
            Program = Application.CreateApplicationInstance();
            Initiator = initiator;

            GUID = Guid.NewGuid();
            StateMachine = new(ProcessState.Unstarted, new StateContext<ProcessState>[]
            {
                new(ProcessState.Unstarted, Array.Empty<ProcessState>(), GotoUnstartedState),
                new(ProcessState.Active, new ProcessState[] { ProcessState.Unstarted }, GotoActiveState),
                new(ProcessState.Stopped, new ProcessState[] { ProcessState.Active }, GotoStoppedState)
            });
        }

        private readonly string[] _args;

        public Guid GUID { get; }

        public TickStateMachine<ProcessState> StateMachine { get; }

        public ProcessState ProcessState => StateMachine.CurrentState;

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

        protected virtual bool GotoUnstartedState(ProcessState sourceState, ProcessState targetState)
        {
            return false;
        }

        protected virtual bool GotoActiveState(ProcessState sourceState, ProcessState targetState)
        {
            Start($"{Application.ID} AppThread");
            return true;
        }

        protected virtual bool GotoStoppedState(ProcessState sourceState, ProcessState targetState)
        {
            Stop();
            return true;
        }

        public void OnTickUpdate(int tick)
        {
            StateMachine.OnTickUpdate(tick);
        }

        protected override void Run()
        {
            string args = _args.Length == 0 ? "empty" : string.Join(", ", _args.Select(arg => $"\"{arg}\""));
            FormContext ? formContext = Initiator is null ? null : MinecraftBlockScreen.Instance.FormContextOf(Initiator);
            if (formContext is null)
                LOGGER.Info($"进程({Application.ID})启动参数为 {args}");
            else
                LOGGER.Info($"进程({Application.ID})启动参数为 {args}，从窗体({formContext.Form.Text})");
            object? result = Program.Main(_args);
            LOGGER.Info($"进程({Application.ID})返回值为 {result ?? "null"}");
        }

        public ProcessContext StartProcess()
        {
            StateMachine.Submit(ProcessState.Active);
            return this;
        }

        public void StopProcess()
        {
            StateMachine.Submit(ProcessState.Stopped);
        }

        public override string ToString()
        {
            return $"GUID={GUID} State={StateMachine.CurrentState} Application={Application.ID}";
        }
    }
}
