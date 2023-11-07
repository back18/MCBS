using log4net.Core;
using log4net.Repository.Hierarchy;
using MCBS.Application;
using MCBS.Cursor;
using MCBS.Cursor.Style;
using MCBS.Logging;
using MCBS.Processes;
using MCBS.Screens;
using MCBS.State;
using MCBS.UI;
using SixLabors.ImageSharp;
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
    public class FormContext : ITickable
    {
        private static readonly LogImpl LOGGER = LogUtil.GetLogger();

        internal FormContext(IProgram program, IForm form)
        {
            Program = program ?? throw new ArgumentNullException(nameof(program));
            Form = form ?? throw new ArgumentNullException(nameof(form));

            if (form is IRootForm rootForm1)
            {
                RootForm = rootForm1;
            }
            else
            {
                MCOS os = MCOS.Instance;
                IForm? initiator = os.ProcessContextOf(Program)?.Initiator;
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
                new(FormState.Active, new FormState[] { FormState.NotLoaded, FormState.Minimize, FormState.Dragging, FormState.Stretching }, HandleActiveState),
                new(FormState.Minimize, new FormState[] { FormState.Active }, HandleMinimizeState),
                new(FormState.Dragging, new FormState[] { FormState.Active }, HandleDraggingState, OnDraggingState),
                new(FormState.Stretching, new FormState[] { FormState.Active }, HandleStretchingState, OnStretchingState),
                new(FormState.Closed, new FormState[] { FormState.Active, FormState.Minimize }, HandleClosedState)
            });

            _closeSemaphore = new(0);
            _closeTask = GetCloseTask();
        }

        private readonly SemaphoreSlim _closeSemaphore;

        private readonly Task _closeTask;

        public int ID { get; internal set; }

        public StateManager<FormState> StateManager { get; }

        public FormState FormState => StateManager.CurrentState;

        public DraggingContext? DraggingContext { get; private set; }

        public StretchingContext? StretchingContext { get; private set; }

        public IProgram Program { get; }

        public IRootForm RootForm { get; private set; }

        public IForm Form { get; }

        protected virtual bool HandleNotLoadedState(FormState current, FormState next)
        {
            return false;
        }

        protected virtual bool HandleActiveState(FormState current, FormState next)
        {
            switch (current)
            {
                case FormState.NotLoaded:
                    if (Form is IRootForm)
                        Form.HandleAllInitialize();
                    else if (!RootForm.ContainsForm(Form))
                        RootForm.AddForm(Form);
                    Form.HandleFormLoad(EventArgs.Empty);
                    ProcessContext? processContext = MCOS.Instance.ProcessContextOf(Program);
                    if (processContext is null)
                        LOGGER.Info($"窗体({Form.Text} #{ID})已打开");
                    else
                        LOGGER.Info($"窗体({Form.Text} #{ID})已被进程({processContext.Application.ID} #{processContext.ID})打开，位于{MCOS.Instance.ScreenContextOf(Form)?.ID ?? -1}号屏幕");
                    return true;
                case FormState.Minimize:
                    if (Form is IRootForm)
                        return false;
                    if (!RootForm.ContainsForm(Form))
                        RootForm.AddForm(Form);
                    Form.HandleFormUnminimize(EventArgs.Empty);
                    LOGGER.Info($"窗体({Form.Text} #{ID})已取消最小化");
                    return true;
                case FormState.Dragging:
                    if (Form is IRootForm)
                        return false;
                    if (DraggingContext is null)
                        return false;
                    if (!DraggingContext.CursorContext.HoverControls.TryRemove(Form, out var hoverControl))
                        return false;
                    if (!RootForm.ContainsForm(Form))
                        RootForm.AddForm(Form);
                    Point position = DraggingContext.CursorContext.NewInputData.CursorPosition;
                    Point offset = hoverControl.OffsetPosition;
                    Form.ClientLocation = new(position.X - offset.X - Form.BorderWidth, position.Y - offset.Y - Form.BorderWidth);
                    DraggingContext = null;
                    LOGGER.Info($"窗体({Form.Text} #{ID})已被拖动到{MCOS.Instance.ScreenContextOf(RootForm)?.ID ?? -1}号屏幕");
                    return true;
                case FormState.Stretching:
                    if (Form is IRootForm)
                        return false;
                    if (StretchingContext is null)
                        return false;
                    StretchingContext = null;
                    return true;
                default:
                    return false;
            }
        }

        protected virtual bool HandleMinimizeState(FormState current, FormState next)
        {
            if (Form is IRootForm)
                return false;
            if (RootForm.ContainsForm(Form))
                RootForm.RemoveForm(Form);
            Form.HandleFormMinimize(EventArgs.Empty);
            LOGGER.Info($"窗体({Form.Text} #{ID})已最小化");
            return true;
        }
        protected virtual bool HandleDraggingState(FormState current, FormState next)
        {
            if (Form is IRootForm)
                return false;
            if (DraggingContext is null)
                return false;
            if (!DraggingContext.CursorContext.HoverControls.TryAdd(Form, DraggingContext.OffsetPosition, out _))
                return false;
            if (RootForm.ContainsForm(Form))
                RootForm.RemoveForm(Form);
            LOGGER.Info($"窗体({Form.Text} #{ID})已从{MCOS.Instance.ScreenContextOf(RootForm)?.ID ?? -1}号屏幕脱离，开始拖动");
            return true;
        }

        protected virtual bool HandleStretchingState(FormState current, FormState next)
        {
            if (Form is IRootForm)
                return false;
            if (StretchingContext is null)
                return false;

            return true;
        }

        protected virtual bool HandleClosedState(FormState current, FormState next)
        {
            if (Form is not IRootForm && RootForm.ContainsForm(Form))
                RootForm.RemoveForm(Form);
            Form.HandleFormClose(EventArgs.Empty);
            LOGGER.Info($"窗体({Form.Text} #{ID})已关闭，返回值为 {Form.ReturnValue ?? "null"}");
            _closeSemaphore.Release();
            return true;
        }

        public virtual void OnDraggingState()
        {

        }

        public virtual void OnStretchingState()
        {

        }

        public void OnTick()
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

        public void DragUpForm(CursorContext cursorContext, Point offsetPosition)
        {
            if (cursorContext is null)
                throw new ArgumentNullException(nameof(cursorContext));

            DraggingContext = new(cursorContext, offsetPosition);
            StateManager.AddNextState(FormState.Dragging);
        }

        public void DragDownForm(IRootForm rootForm)
        {
            if (rootForm is null)
                throw new ArgumentNullException(nameof(rootForm));
            if (DraggingContext is null)
                return;

            RootForm = rootForm;
            StateManager.AddNextState(FormState.Active);
        }

        public void StretchUpForm(CursorContext cursorContext, Direction borders)
        {
            if (cursorContext is null)
                throw new ArgumentNullException(nameof(cursorContext));

            StretchingContext = new(cursorContext, borders);
            StateManager.AddNextState(FormState.Stretching);
        }

        public void StretchDownForm()
        {
            if (StretchingContext is null)
                return;

            StateManager.AddNextState(FormState.Active);
        }

        public void WaitForClose()
        {
            _closeTask.Wait();
        }

        public async Task WaitForCloseAsync()
        {
            await _closeTask;
        }

        private async Task GetCloseTask()
        {
            await _closeSemaphore.WaitAsync();
        }

        public override string ToString()
        {
            return $"State={FormState} FID={ID}, PID={MCOS.Instance.ProcessContextOf(Form)?.ID}, SID = {MCOS.Instance.ScreenContextOf(Form)?.ID}, Form=[{Form}]";
        }
    }
}
