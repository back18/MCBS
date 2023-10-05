using static MCBS.Config.ConfigManager;
using log4net.Core;
using NAudio.Codecs;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MCBS.Logging;
using MCBS.UI;
using MCBS.State;
using MCBS.Events;

namespace MCBS.Screens
{
    /// <summary>
    /// 屏幕运行时上下文
    /// </summary>
    public class ScreenContext
    {
        private static readonly LogImpl LOGGER = LogUtil.GetLogger();

        internal ScreenContext(Screen screen, IRootForm form)
        {
            Screen = screen ?? throw new ArgumentNullException(nameof(screen));
            RootForm = form ?? throw new ArgumentNullException(nameof(form));
            IsRestart = false;
            IsShowCursor = true;
            CursorType = Cursor.CursorStyleType.Default;

            ID = -1;
            StateManager = new(ScreenState.NotLoaded, new StateContext<ScreenState>[]
            {
                new(ScreenState.NotLoaded, Array.Empty<ScreenState>(), HandleNotLoadedState),
                new(ScreenState.Active, new ScreenState[] { ScreenState.NotLoaded }, HandleActiveState),
                new(ScreenState.Sleep, new ScreenState[] { ScreenState.Active }, HandleSleepState),
                new(ScreenState.Closed, new ScreenState[] { ScreenState.Active, ScreenState.Sleep }, HandleClosedState)
            });

            _bind = false;
        }

        private bool _bind;

        public int ID { get; internal set; }

        public StateManager<ScreenState> StateManager { get; }

        public ScreenState ScreenState => StateManager.CurrentState;

        public Screen Screen { get; }

        public IRootForm RootForm { get; set; }

        public bool IsRestart { get; private set; }

        public bool IsShowCursor { get; set; }

        public string CursorType { get; set; }

        protected virtual bool HandleNotLoadedState(ScreenState current, ScreenState next)
        {
            return false;
        }

        protected virtual bool HandleActiveState(ScreenState current, ScreenState next)
        {
            switch (current)
            {
                case ScreenState.NotLoaded:
                    IsRestart = false;
                    Screen.Start();
                    RootForm.ClientSize = Screen.Size;
                    MCOS.Instance.RunStartupChecklist(RootForm);
                    BindEvents();
                    LOGGER.Info($"屏幕({Screen.StartPosition} #{ID})已加载");
                    return true;
                case ScreenState.Sleep:
                    //TODO
                    return false;
                default:
                    return false;
            }
        }

        protected virtual bool HandleSleepState(ScreenState current, ScreenState next)
        {
            //TODO
            return false;
        }

        protected virtual bool HandleClosedState(ScreenState current, ScreenState next)
        {
            UnbindEvents();
            foreach (var forem in MCOS.Instance.FormManager.Items.Values)
            {
                if (forem.RootForm == RootForm)
                    forem.CloseForm();
            }
            RootForm.CloseForm();
            Screen.Stop();
            LOGGER.Info($"屏幕({Screen.StartPosition} #{ID})已卸载");
            return true;
        }

        public void Handle()
        {
            StateManager.HandleAllState();
        }

        public ScreenContext LoadScreen()
        {
            StateManager.AddNextState(ScreenState.Active);
            return this;
        }

        public void CloseScreen()
        {
            StateManager.AddNextState(ScreenState.Closed);
        }

        public void RestartScreen()
        {
            StateManager.AddNextState(ScreenState.Closed);
            IsRestart = true;
        }

        public void StartSleep()
        {
            StateManager.AddNextState(ScreenState.Sleep);
        }

        public void StopSleep()
        {
            StateManager.AddNextState(ScreenState.Active);
        }

        public override string ToString()
        {
            return $"State={ScreenState}, SID={ID}, Screen=[{Screen}]";
        }

        internal void BindEvents()
        {
            if (_bind)
                return;

            Screen.InputHandler.CursorMove += InputHandler_CursorMove;
            Screen.InputHandler.RightClick += InputHandler_RightClick;
            Screen.InputHandler.LeftClick += InputHandler_LeftClick;
            Screen.InputHandler.CursorSlotChanged += InputHandler_CursorSlotChanged;
            Screen.InputHandler.CursorItemChanged += InputHandler_CursorItemChanged;
            Screen.InputHandler.TextEditorUpdate += InputHandler_TextEditorUpdate;

            _bind = true;
        }

        internal void UnbindEvents()
        {
            if (!_bind)
                return;

            Screen.InputHandler.CursorMove -= InputHandler_CursorMove;
            Screen.InputHandler.RightClick -= InputHandler_RightClick;
            Screen.InputHandler.LeftClick -= InputHandler_LeftClick;
            Screen.InputHandler.CursorSlotChanged -= InputHandler_CursorSlotChanged;
            Screen.InputHandler.CursorItemChanged -= InputHandler_CursorItemChanged;
            Screen.InputHandler.TextEditorUpdate -= InputHandler_TextEditorUpdate;

            _bind = false;
        }

        private void InputHandler_CursorMove(ICursorReader sender, CursorEventArgs e)
        {
            RootForm.HandleCursorMove(e);
        }

        private void InputHandler_RightClick(ICursorReader sender, CursorEventArgs e)
        {
            RootForm.HandleRightClick(e);
        }

        private void InputHandler_CursorSlotChanged(ICursorReader sender, CursorSlotEventArgs e)
        {
            RootForm.HandleCursorSlotChanged(e);
        }

        private void InputHandler_LeftClick(ICursorReader sender, CursorEventArgs e)
        {
            RootForm.HandleLeftClick(e);
        }

        private void InputHandler_CursorItemChanged(ICursorReader sender, CursorItemEventArgs e)
        {
            RootForm.HandleCursorItemChanged(e);
        }

        private void InputHandler_TextEditorUpdate(ITextEditor sender, CursorTextEventArgs e)
        {
            RootForm.HandleTextEditorUpdate(e);
        }
    }
}
