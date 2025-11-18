using log4net.Core;
using MCBS.Application;
using MCBS.Cursor;
using MCBS.Processes;
using MCBS.Screens;
using MCBS.UI;
using QuanLib.Core;
using QuanLib.Game;
using QuanLib.Logging;
using QuanLib.TickLoop;
using QuanLib.TickLoop.StateMachine;
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
    public class FormContext : UnmanagedBase, ITickUpdatable
    {
        private static readonly LogImpl LOGGER = LogManager.Instance.GetLogger();

        internal FormContext(IProgram program, IForm form)
        {
            ArgumentNullException.ThrowIfNull(program, nameof(program));
            ArgumentNullException.ThrowIfNull(form, nameof(form));

            Program = program;
            Form = form;

            if (form is IRootForm rootForm1)
            {
                RootForm = rootForm1;
            }
            else
            {
                MinecraftBlockScreen mcbs = MinecraftBlockScreen.Instance;
                IForm? initiator = mcbs.ProcessContextOf(Program)?.Initiator;
                if (initiator is IRootForm rootForm2)
                {
                    RootForm = rootForm2;
                }
                else
                {
                    ScreenContext? screenContext = null;
                    if (initiator is not null)
                        screenContext = mcbs.ScreenContextOf(initiator);

                    if (screenContext is not null)
                        RootForm = screenContext.RootForm;
                    else if (mcbs.ScreenManager.Items.Count != 0)
                        RootForm = mcbs.ScreenManager.Items.FirstOrDefault().Value.RootForm;
                    else
                        throw new InvalidOperationException();
                }
            }

            GUID = Guid.NewGuid();
            StateMachine = new(FormState.NotLoaded, new StateContext<FormState>[]
            {
                new(FormState.NotLoaded, Array.Empty<FormState>(), GotoNotLoadedState),
                new(FormState.Active, new FormState[] { FormState.NotLoaded, FormState.Minimize, FormState.Dragging, FormState.Stretching }, GotoActiveState),
                new(FormState.Minimize, new FormState[] { FormState.Active }, GotoMinimizeState),
                new(FormState.Dragging, new FormState[] { FormState.Active }, GotoDraggingState),
                new(FormState.Stretching, new FormState[] { FormState.Active }, GotoStretchingState),
                new(FormState.Closed, new FormState[] { FormState.Active, FormState.Minimize }, GotoClosedState)
            });

            _closeSemaphore = new();
        }

        private readonly TaskSemaphore _closeSemaphore;

        public Guid GUID { get; }

        public TickStateMachine<FormState> StateMachine { get; }

        public FormState FormState => StateMachine.CurrentState;

        public DraggingContext? DraggingContext { get; private set; }

        public StretchingContext? StretchingContext { get; private set; }

        public IProgram Program { get; }

        public IRootForm RootForm { get; private set; }

        public IForm Form { get; }

        protected virtual bool GotoNotLoadedState(FormState sourceState, FormState targetState)
        {
            return false;
        }

        protected virtual bool GotoActiveState(FormState sourceState, FormState targetState)
        {
            switch (sourceState)
            {
                case FormState.NotLoaded:
                    if (!RootForm.ContainsForm(Form))
                        RootForm.AddForm(Form);
                    Form.HandleFormLoad(EventArgs.Empty);
                    ProcessContext? processContext = MinecraftBlockScreen.Instance.ProcessContextOf(Program);
                    if (processContext is null)
                        LOGGER.Info($"窗体({Form.Text})已打开");
                    else
                        LOGGER.Info($"窗体({Form.Text})已被进程({processContext.Application.ID})打开，位于屏幕({MinecraftBlockScreen.Instance.ScreenContextOf(Form)?.Screen.StartPosition})");
                    return true;
                case FormState.Minimize:
                    if (Form is IRootForm)
                        return false;
                    if (!RootForm.ContainsForm(Form))
                        RootForm.AddForm(Form);
                    Form.HandleFormUnminimize(EventArgs.Empty);
                    LOGGER.Info($"窗体({Form.Text})已取消最小化");
                    return true;
                case FormState.Dragging:
                    if (DraggingContext is null)
                        return false;
                    if (!DraggingContext.CursorContext.HoverControls.TryRemove(Form, out var hoverControl))
                        return false;
                    if (!RootForm.ContainsForm(Form))
                        RootForm.AddForm(Form);
                    Point position = DraggingContext.CursorContext.NewInputData.CursorPosition;
                    Point offset = hoverControl.OffsetPosition;
                    Form.ClientLocation = new(
                        position.X - offset.X - RootForm.ClientLocation.X - RootForm.BorderWidth - Form.BorderWidth,
                        position.Y - offset.Y - RootForm.ClientLocation.Y - RootForm.BorderWidth - Form.BorderWidth);
                    DraggingContext = null;
                    LOGGER.Info($"窗体({Form.Text})已被拖动到屏幕({MinecraftBlockScreen.Instance.ScreenContextOf(RootForm)?.Screen.StartPosition})");
                    return true;
                case FormState.Stretching:
                    if (StretchingContext is null)
                        return false;
                    StretchingContext = null;
                    return true;
                default:
                    return false;
            }
        }

        protected virtual bool GotoMinimizeState(FormState sourceState, FormState targetState)
        {
            if (Form is IRootForm)
                return false;
            if (RootForm.ContainsForm(Form))
                RootForm.RemoveForm(Form);
            Form.HandleFormMinimize(EventArgs.Empty);
            LOGGER.Info($"窗体({Form.Text})已最小化");
            return true;
        }

        protected virtual bool GotoDraggingState(FormState sourceState, FormState targetState)
        {
            if (Form is IRootForm rootForm && !rootForm.AllowDrag)
                return false;
            if (DraggingContext is null)
                return false;
            if (!DraggingContext.CursorContext.HoverControls.TryAdd(Form, DraggingContext.OffsetPosition, out _))
                return false;
            if (RootForm.ContainsForm(Form))
                RootForm.RemoveForm(Form);
            LOGGER.Info($"窗体({Form.Text})已从屏幕({MinecraftBlockScreen.Instance.ScreenContextOf(RootForm)?.Screen.StartPosition})脱离，开始拖动");
            return true;
        }

        protected virtual bool GotoStretchingState(FormState sourceState, FormState targetState)
        {
            if (Form is IRootForm rootForm && !rootForm.AllowSelected)
                return false;
            if (StretchingContext is null)
                return false;

            return true;
        }

        protected virtual bool GotoClosedState(FormState sourceState, FormState targetState)
        {
            Dispose();
            return true;
        }

        public void OnTickUpdate(int tick)
        {
            StateMachine.OnTickUpdate(tick);
        }

        public FormContext LoadForm()
        {
            StateMachine.Submit(FormState.Active);
            return this;
        }

        public void CloseForm()
        {
            StateMachine.Submit(FormState.Closed);
        }

        public void MinimizeForm()
        {
            StateMachine.Submit(FormState.Minimize);
        }

        public void UnminimizeForm()
        {
            StateMachine.Submit(FormState.Active);
        }

        public void DragUpForm(CursorContext cursorContext, Point offsetPosition)
        {
            ArgumentNullException.ThrowIfNull(cursorContext, nameof(cursorContext));

            DraggingContext = new(cursorContext, offsetPosition);
            StateMachine.Submit(FormState.Dragging);
        }

        public void DragDownForm(IRootForm rootForm)
        {
            ArgumentNullException.ThrowIfNull(rootForm, nameof(rootForm));
            if (DraggingContext is null)
                return;

            RootForm = rootForm;
            StateMachine.Submit(FormState.Active);
        }

        public void StretchUpForm(CursorContext cursorContext, Direction borders)
        {
            ArgumentNullException.ThrowIfNull(cursorContext, nameof(cursorContext));

            StretchingContext = new(cursorContext, borders);
            StateMachine.Submit(FormState.Stretching);
        }

        public void StretchDownForm()
        {
            if (StretchingContext is null)
                return;

            StateMachine.Submit(FormState.Active);
        }

        public void WaitForClose()
        {
            _closeSemaphore.Wait();
        }

        public Task WaitForCloseAsync()
        {
            return _closeSemaphore.WaitAsync();
        }

        public override string ToString()
        {
            return $"GUID={GUID} State={StateMachine.CurrentState} Title={Form.Text} Position=[{Form.ClientLocation.X},{Form.ClientLocation.Y}] Size=({Form.ClientSize.Width},{Form.ClientSize.Height})";
        }

        protected override void DisposeUnmanaged()
        {
            if (FormState == FormState.Closed || FormState == FormState.NotLoaded)
                return;

            if (FormState == FormState.Dragging && DraggingContext is not null)
            {
                DraggingContext.CursorContext.HoverControls.TryRemove(Form, out _);
                DraggingContext = null;
            }

            if (FormState == FormState.Stretching)
                StretchingContext = null;

            if (Form is not IRootForm && RootForm.ContainsForm(Form))
                RootForm.RemoveForm(Form);

            Form.HandleFormClose(EventArgs.Empty);
            LOGGER.Info($"窗体({Form.Text})已关闭，返回值为 {Form.ReturnValue ?? "null"}");
            _closeSemaphore.Release();
        }
    }
}
