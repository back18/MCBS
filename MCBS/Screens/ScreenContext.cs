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
using MCBS.Frame;
using QuanLib.Minecraft.Snbt.Models;

namespace MCBS.Screens
{
    /// <summary>
    /// 屏幕运行时上下文
    /// </summary>
    public class ScreenContext : ITickable
    {
        private static readonly LogImpl LOGGER = LogUtil.GetLogger();

        internal ScreenContext(Screen screen, IRootForm form)
        {
            Screen = screen ?? throw new ArgumentNullException(nameof(screen));
            RootForm = form ?? throw new ArgumentNullException(nameof(form));
            ScreenInputHandler = new(this);
            IsRestart = false;

            ID = -1;
            StateManager = new(ScreenState.NotLoaded, new StateContext<ScreenState>[]
            {
                new(ScreenState.NotLoaded, Array.Empty<ScreenState>(), HandleNotLoadedState),
                new(ScreenState.Active, new ScreenState[] { ScreenState.NotLoaded }, HandleActiveState),
                new(ScreenState.Sleep, new ScreenState[] { ScreenState.Active }, HandleSleepState),
                new(ScreenState.Unload, new ScreenState[] { ScreenState.Active, ScreenState.Sleep }, HandleUnloadState)
            });

            _frame = null;
            _cursors = new();
        }

        private ArrayFrame? _frame;

        private readonly List<CursorContext> _cursors;

        public int ID { get; internal set; }

        public StateManager<ScreenState> StateManager { get; }

        public ScreenState ScreenState => StateManager.CurrentState;

        public Screen Screen { get; }

        public IRootForm RootForm { get; }

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

        protected virtual bool HandleUnloadState(ScreenState current, ScreenState next)
        {
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

        public void OnTick()
        {
            StateManager.HandleAllState();
        }

        public async Task HandleScreenInputAsync()
        {
            await Task.Run(() =>
            {
                CursorContext[] cursors = ScreenInputHandler.HandleInput();
                _cursors.Clear();
                _cursors.AddRange(cursors);
            });
        }

        public async Task HandleScreenEventAsync()
        {
            await Task.Run(() =>
            {
                foreach (var cursorContext in _cursors)
                {
                    if (cursorContext.ScreenContextOf == this && cursorContext.CursorState == CursorState.Active)
                        InvokeScreenEvent(cursorContext);
                }
            });
        }

        public async Task HandleBeforeFrameAsync()
        {
            await Task.Run(() => RootForm.HandleBeforeFrame(EventArgs.Empty));
        }

        public async Task HandleUIRenderingAsync()
        {
            ArrayFrame baseFrame = ArrayFrame.BuildFrame(Screen.Width, Screen.Height, Screen.DefaultBackgroundBlcokID);
            ArrayFrame? formFrame = await UIRenderer.RenderingAsync(RootForm);
            if (formFrame is not null)
                baseFrame.Overwrite(formFrame, RootForm.ClientLocation);
            foreach (var cursorContext in _cursors)
            {
                if (cursorContext.ScreenContextOf == this && cursorContext.CursorState == CursorState.Active)
                {
                    if (cursorContext.HoverControl is not null)
                    {
                        ArrayFrame? hoverFrame = await UIRenderer.RenderingAsync(cursorContext.HoverControl);
                        if (hoverFrame is not null)
                        {
                            baseFrame.Overwrite(hoverFrame, cursorContext.NewInputData.CursorPosition, cursorContext.HoverControl.OffsetPosition);
                            baseFrame.DrawBorder(cursorContext.HoverControl, cursorContext.NewInputData.CursorPosition, cursorContext.HoverControl.OffsetPosition);
                        }
                    }

                    if (!SR.CursorStyleManager.TryGetValue(cursorContext.StyleType, out var cursorStyle))
                        cursorStyle = SR.CursorStyleManager[CursorStyleType.Default];
                    baseFrame.Overwrite(cursorStyle.Frame, cursorContext.NewInputData.CursorPosition, cursorStyle.Offset);
                }
            }
            _frame = baseFrame;
        }

        public async Task HandleScreenOutputAsync()
        {
            if (_frame is null)
                return;
            await Screen.OutputHandler.HandleOutputAsync(_frame);
        }

        public async Task HandleAfterFrameAsync()
        {
            await Task.Run(() => RootForm.HandleAfterFrame(EventArgs.Empty));
        }

        public ScreenContext LoadScreen()
        {
            StateManager.AddNextState(ScreenState.Active);
            return this;
        }

        public void UnloadScreen()
        {
            StateManager.AddNextState(ScreenState.Unload);
        }

        public void RestartScreen()
        {
            StateManager.AddNextState(ScreenState.Unload);
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

        private void InvokeScreenEvent(CursorContext cursorContext)
        {
            if (cursorContext is null)
                throw new ArgumentNullException(nameof(cursorContext));

            CursorInputData oldData = cursorContext.OldInputData;
            CursorInputData newData = cursorContext.NewInputData;
            if (oldData.CursorPosition != newData.CursorPosition)
                RootForm.HandleCursorMove(new(newData.CursorPosition, cursorContext));
            if (oldData.LeftClickTime != newData.LeftClickTime)
                RootForm.HandleLeftClick(new(newData.CursorPosition, cursorContext));
            if (oldData.RightClickTime != newData.RightClickTime)
                RootForm.HandleRightClick(new(newData.CursorPosition, cursorContext));
            if (oldData.TextEditor != newData.TextEditor)
                RootForm.HandleTextEditorUpdate(new(newData.CursorPosition, cursorContext));
            if (oldData.InventorySlot != newData.InventorySlot)
                RootForm.HandleCursorSlotChanged(new(newData.CursorPosition, cursorContext));
            if (!Item.EqualsID(oldData.DeputyItem, newData.DeputyItem))
                RootForm.HandleCursorItemChanged(new(newData.CursorPosition, cursorContext));
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
