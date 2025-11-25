using MCBS.Application;
using MCBS.Forms;
using MCBS.ObjectModel;
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
    public class ProcessContext : RunnableBase, IUnique, ITickUpdatable
    {
        private static readonly ILogger LOGGER = Log4NetManager.Instance.GetLogger();

        internal ProcessContext(ApplicationManifest applicationManifest, Guid guid, string[] args, IForm? initiator = null) : base(Log4NetManager.Instance.GetProvider())
        {
            ArgumentNullException.ThrowIfNull(applicationManifest, nameof(applicationManifest));
            ArgumentNullException.ThrowIfNull(args, nameof(args));

            if (guid == default)
                throw new ArgumentException("无效的GUID", nameof(guid));

            _args = args;
            Application = applicationManifest;
            Program = Application.CreateApplicationInstance();
            Initiator = initiator;

            Guid = guid;
            StateMachine = new(ProcessState.Unstarted, new StateContext<ProcessState>[]
            {
                new(ProcessState.Unstarted, Array.Empty<ProcessState>(), GotoUnstartedState),
                new(ProcessState.Active, new ProcessState[] { ProcessState.Unstarted }, GotoActiveState),
                new(ProcessState.Stopped, new ProcessState[] { ProcessState.Active }, GotoStoppedState)
            });
        }

        private readonly string[] _args;

        public Guid Guid { get; }

        public string ShortId => Guid.GetShortId();

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
            Start("Application Thread #" + ShortId);
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
            string? args = _args.Length == 0 ? null : string.Join(", ", _args.Select(arg => $"\"{arg}\""));
            FormContext? formContext = Initiator is null ? null : MinecraftBlockScreen.Instance.FormContextOf(Initiator);

            if (formContext is not null && args is not null)
                LOGGER.Info($"进程({ShortId})启动参数为 {args}，从窗体({formContext.ShortId})");

            object? result = Program.Main(_args);

            if (result is not null && result is not 0)
                LOGGER.Info($"进程({ShortId})返回值为 {ObjectFormatter.Format(result)}");
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
            return $"Id={ShortId} State={StateMachine.CurrentState} Application={Application.ID}";
        }
    }
}
