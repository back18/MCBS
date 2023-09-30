using log4net.Core;
using log4net.Repository.Hierarchy;
using MCBS.Application;
using MCBS.Logging;
using MCBS.Processes;
using MCBS.Screens;
using MCBS.State;
using MCBS.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Forms
{
    /// <summary>
    /// 窗体运行时上下文
    /// </summary>
    public class FormContext
    {
        private static readonly LogImpl LOGGER = LogUtil.GetLogger();

        internal FormContext(ApplicationBase application, IForm form)
        {
            Application = application ?? throw new ArgumentNullException(nameof(application));
            Form = form ?? throw new ArgumentNullException(nameof(form));

            if (form is IRootForm rootForm1)
            {
                RootForm = rootForm1;
            }
            else
            {
                MCOS os = MCOS.Instance;
                IForm? initiator = os.ProcessOf(Application)?.Initiator;
                if (initiator is IRootForm rootForm2)
                {
                    RootForm = rootForm2;
                }
                else
                {
                    ScreenContext? screenContext = null;
                    if (initiator is not null)
                        screenContext = os.ScreenContextOf(initiator);

                    if (screenContext is not null)
                        RootForm = screenContext.RootForm;
                    else if (os.ScreenManager.Items.Any())
                        RootForm = os.ScreenManager.Items.FirstOrDefault().Value.RootForm;
                    else
                        throw new InvalidOperationException();
                }
            }

            ID = -1;
            StateManager = new(FormState.NotLoaded, new StateContext<FormState>[]
            {
                new(FormState.NotLoaded, Array.Empty<FormState>(), HandleNotLoadedState),
                new(FormState.Active, new FormState[] { FormState.NotLoaded, FormState.Minimize }, HandleActiveState),
                new(FormState.Minimize, new FormState[] { FormState.Active }, HandleMinimizeState),
                new(FormState.Closed, new FormState[] { FormState.Active, FormState.Minimize }, HandleClosedState)
            });

            _close = new(false);
        }

        private readonly AutoResetEvent _close;

        public int ID { get; internal set; }

        public StateManager<FormState> StateManager { get; }

        public FormState FormState => StateManager.CurrentState;

        public IRootForm RootForm { get; private set; }

        public ApplicationBase Application { get; }

        public IForm Form { get; }

        protected bool HandleNotLoadedState(FormState current, FormState next)
        {
            return false;
        }

        protected bool HandleActiveState(FormState current, FormState next)
        {
            switch (current)
            {
                case FormState.NotLoaded:
                    if (Form is IRootForm)
                        Form.HandleAllInitialize();
                    else if (!RootForm.ContainsForm(Form))
                        RootForm.AddForm(Form);
                    Form.HandleFormLoad(EventArgs.Empty);
                    ProcessContext? processContext = MCOS.Instance.ProcessOf(Application);
                    if (processContext is null)
                        LOGGER.Info($"窗体({Form.Text} #{ID})已打开");
                    else
                        LOGGER.Info($"窗体({Form.Text} #{ID})已被进程({processContext.ApplicationInfo.ID} #{processContext.ID})打开");
                    return true;
                case FormState.Minimize:
                    if (Form is IRootForm)
                        return false;
                    if (!RootForm.ContainsForm(Form))
                        RootForm.AddForm(Form);
                    Form.HandleFormUnminimize(EventArgs.Empty);
                    LOGGER.Info($"窗体({Form.Text} #{ID})已取消最小化");
                    return true;
                default:
                    return false;
            }
        }

        protected bool HandleMinimizeState(FormState current, FormState next)
        {
            if (Form is IRootForm)
                return false;
            if (RootForm.ContainsForm(Form))
                RootForm.RemoveForm(Form);
            Form.HandleFormMinimize(EventArgs.Empty);
            LOGGER.Info($"窗体({Form.Text} #{ID})已最小化");
            return true;
        }

        protected bool HandleClosedState(FormState current, FormState next)
        {
            if (Form is not IRootForm && RootForm.ContainsForm(Form))
                RootForm.RemoveForm(Form);
            Form.HandleFormClose(EventArgs.Empty);
            LOGGER.Info($"窗体({Form.Text} #{ID})已关闭，返回值为 {Form.ReturnValue ?? "null"}");
            _close.Set();
            return true;
        }

        public void Handle()
        {
            StateManager.HandleAllState();
        }

        public FormContext LoadForm()
        {
            StateManager.AddNextState(FormState.Active);
            return this;
        }

        public void CloseForm()
        {
            StateManager.AddNextState(FormState.Closed);
        }

        public void MinimizeForm()
        {
            StateManager.AddNextState(FormState.Minimize);
        }

        public void UnminimizeForm()
        {
            StateManager.AddNextState(FormState.Active);
        }

        public void WaitForFormClose()
        {
            _close.WaitOne();
        }

        public override string ToString()
        {
            return $"State={FormState} FID={ID}, PID={MCOS.Instance.ProcessOf(Form)?.ID}, SID = {MCOS.Instance.ScreenContextOf(Form)?.ID}, Form=[{Form}]";
        }
    }
}
