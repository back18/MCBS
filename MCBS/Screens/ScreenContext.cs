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
using MCBS.Cursor.Style;
using MCBS.Cursor;

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
            CursorManager = new();
            ScreenInputHandler = new(this);
            IsRestart = false;

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

        public IRootForm RootForm { get; }

        public CursorManager CursorManager { get; }

        public ScreenInputHandler ScreenInputHandler { get; }

        public bool IsRestart { get; private set; }

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

            ScreenInputHandler.CursorMove += ScreenInputHandler_CursorMove;
            ScreenInputHandler.LeftClick += ScreenInputHandler_LeftClick;
            ScreenInputHandler.RightClick += ScreenInputHandler_RightClick;
            ScreenInputHandler.TextEditorUpdate += ScreenInputHandler_TextEditorUpdate;
            ScreenInputHandler.CursorSlotChanged += ScreenInputHandler_CursorSlotChanged;
            ScreenInputHandler.CursorItemChanged += ScreenInputHandler_CursorItemChanged;

            _bind = true;
        }

        internal void UnbindEvents()
        {
            if (!_bind)
                return;

            ScreenInputHandler.CursorMove -= ScreenInputHandler_CursorMove;
            ScreenInputHandler.LeftClick -= ScreenInputHandler_LeftClick;
            ScreenInputHandler.RightClick -= ScreenInputHandler_RightClick;
            ScreenInputHandler.TextEditorUpdate -= ScreenInputHandler_TextEditorUpdate;
            ScreenInputHandler.CursorSlotChanged -= ScreenInputHandler_CursorSlotChanged;
            ScreenInputHandler.CursorItemChanged -= ScreenInputHandler_CursorItemChanged;

            _bind = false;
        }

        private void ScreenInputHandler_CursorMove(ScreenInputHandler sender, CursorEventArgs e)
        {
            RootForm.HandleCursorMove(e);
        }

        private void ScreenInputHandler_LeftClick(ScreenInputHandler sender, CursorEventArgs e)
        {
            RootForm.HandleLeftClick(e);
        }

        private void ScreenInputHandler_RightClick(ScreenInputHandler sender, CursorEventArgs e)
        {
            RootForm.HandleRightClick(e);
        }

        private void ScreenInputHandler_TextEditorUpdate(ScreenInputHandler sender, CursorEventArgs e)
        {
            RootForm.HandleTextEditorUpdate(e);
        }

        private void ScreenInputHandler_CursorSlotChanged(ScreenInputHandler sender, CursorEventArgs e)
        {
            RootForm.HandleCursorSlotChanged(e);
        }

        private void ScreenInputHandler_CursorItemChanged(ScreenInputHandler sender, CursorEventArgs e)
        {
            RootForm.HandleCursorItemChanged(e);
        }
    }
}
