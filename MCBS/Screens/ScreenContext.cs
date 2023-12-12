﻿using static MCBS.Config.ConfigManager;
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
using QuanLib.Minecraft.Snbt.Models;
using MCBS.Rendering;

namespace MCBS.Screens
{
    /// <summary>
    /// 屏幕运行时上下文
    /// </summary>
    public class ScreenContext : ITickable
    {
        private static readonly LogImpl LOGGER = LogUtil.GetLogger();

        internal ScreenContext(Screen screen, IScreenView form)
        {
            ArgumentNullException.ThrowIfNull(screen, nameof(screen));
            ArgumentNullException.ThrowIfNull(form, nameof(form));

            Screen = screen;
            ScreenView = form;
            ScreenInputHandler = new(this);
            ScreenOutputHandler = new(this);
            IsRestarting = false;

            ID = -1;
            StateManager = new(ScreenState.NotLoaded, new StateContext<ScreenState>[]
            {
                new(ScreenState.NotLoaded, Array.Empty<ScreenState>(), HandleNotLoadedState),
                new(ScreenState.Active, new ScreenState[] { ScreenState.NotLoaded }, HandleActiveState),
                new(ScreenState.Sleep, new ScreenState[] { ScreenState.Active }, HandleSleepState),
                new(ScreenState.Unload, new ScreenState[] { ScreenState.Active, ScreenState.Sleep }, HandleUnloadState)
            });

            _frame = null;
            _activeCursors = new();
            _offlineCursors = new();
        }

        private BlockFrame? _frame;

        private readonly List<CursorContext> _activeCursors;

        private readonly List<CursorContext> _offlineCursors;

        public int ID { get; internal set; }

        public StateManager<ScreenState> StateManager { get; }

        public ScreenState ScreenState => StateManager.CurrentState;

        public Screen Screen { get; }

        public IScreenView ScreenView { get; }

        public IRootForm RootForm => ScreenView.RootForm;

        public ScreenInputHandler ScreenInputHandler { get; }

        public ScreenOutputHandler ScreenOutputHandler { get; }

        public bool IsRestarting { get; private set; }

        protected virtual bool HandleNotLoadedState(ScreenState current, ScreenState next)
        {
            return false;
        }

        protected virtual bool HandleActiveState(ScreenState current, ScreenState next)
        {
            switch (current)
            {
                case ScreenState.NotLoaded:
                    IsRestarting = false;
                    Screen.LoadScreenChunks();
                    ScreenView.ClientSize = new(Screen.Width, Screen.Height);
                    ScreenView.HandleBeforeInitialize();
                    ScreenView.HandleInitialize();
                    ScreenView.HandleAfterInitialize();
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
            Screen.UnloadScreenChunks();
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
                _offlineCursors.Clear();
                foreach (var cursor in _activeCursors)
                {
                    if (Array.IndexOf(cursors, cursor) == -1)
                        _offlineCursors.Add(cursor);
                }
                _activeCursors.Clear();
                _activeCursors.AddRange(cursors);
            });
        }

        public async Task HandleScreenEventAsync()
        {
            await Task.Run(() =>
            {
                foreach (var cursorContext in _offlineCursors)
                {
                    if (RootForm.IsHover)
                        RootForm.HandleCursorMove(new(new(-1024, -1024), cursorContext));
                }

                foreach (var cursorContext in _activeCursors)
                {
                    if (cursorContext.ScreenContextOf == this)
                        InvokeScreenEvent(cursorContext);
                }
            });
        }

        public async Task HandleBeforeFrameAsync()
        {
            await Task.Run(() => ScreenView.HandleBeforeFrame(EventArgs.Empty));
        }

        public async Task HandleUIRenderingAsync()
        {
            HashBlockFrame baseFrame = new(Screen.Width, Screen.Height, ScreenOutputHandler.ScreenDefaultBlock);
            BlockFrame formFrame = await ScreenView.GetRenderingResultAsync();
            baseFrame.Overwrite(formFrame, ScreenView.ClientSize, ScreenView.ClientLocation, ScreenView.OffsetPosition);
            foreach (var cursorContext in _activeCursors)
            {
                if (cursorContext.ScreenContextOf == this)
                {
                    Point position = cursorContext.NewInputData.CursorPosition;

                    foreach (HoverControl hoverControl in cursorContext.HoverControls.Values)
                    {
                        BlockFrame hoverFrame = await hoverControl.Control.GetRenderingResultAsync();
                        if (hoverFrame is not null)
                        {
                            Point offset = hoverControl.OffsetPosition;
                            baseFrame.Overwrite(hoverFrame, hoverControl.Control.ClientSize, new(position.X - offset.X, position.Y - offset.Y));
                            baseFrame.DrawBorder(hoverControl.Control, position, hoverControl.OffsetPosition);
                        }
                    }

                    if (cursorContext.Visible)
                    {
                        if (!SR.CursorStyleManager.TryGetValue(cursorContext.StyleType, out var cursorStyle))
                            cursorStyle = SR.CursorStyleManager[CursorStyleType.Default];
                        Point offset = cursorStyle.Offset;
                        baseFrame.Overwrite(cursorStyle.BlockFrame, new(position.X - offset.X, position.Y - offset.Y));
                    }
                }
            }
            _frame = baseFrame;
        }

        public async Task HandleScreenOutputAsync()
        {
            if (_frame is null)
                return;
            await ScreenOutputHandler.HandleOutputAsync(_frame);
        }

        public async Task HandleAfterFrameAsync()
        {
            await Task.Run(() => ScreenView.HandleAfterFrame(EventArgs.Empty));
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
            IsRestarting = true;
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
            ArgumentNullException.ThrowIfNull(cursorContext, nameof(cursorContext));

            CursorInputData oldData = cursorContext.OldInputData;
            CursorInputData newData = cursorContext.NewInputData;
            if (oldData.CursorPosition != newData.CursorPosition)
                ScreenView.HandleCursorMove(new(newData.CursorPosition, cursorContext));
            if (oldData.LeftClickTime != newData.LeftClickTime)
                ScreenView.HandleLeftClick(new(newData.CursorPosition, cursorContext));
            if (oldData.RightClickTime != newData.RightClickTime)
                ScreenView.HandleRightClick(new(newData.CursorPosition, cursorContext));
            if (cursorContext.TextEditor.SynchronizeTick != MCOS.Instance.SystemTick && oldData.TextEditor != newData.TextEditor)
                ScreenView.HandleTextEditorUpdate(new(newData.CursorPosition, cursorContext));

            string? deputyItem = cursorContext.NewInputData.DeputyItem?.ID;
            if (deputyItem == ScreenConfig.RightClickItemID || deputyItem == ScreenConfig.TextEditorItemID)
            {
                if (oldData.InventorySlot != newData.InventorySlot)
                    ScreenView.HandleCursorSlotChanged(new(newData.CursorPosition, cursorContext));
                if (!Item.EqualsID(oldData.DeputyItem, newData.DeputyItem))
                    ScreenView.HandleCursorItemChanged(new(newData.CursorPosition, cursorContext));
            }
        }
    }
}
