using static MCBS.Config.ConfigManager;
using log4net.Core;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MCBS.UI;
using MCBS.Cursor.Style;
using MCBS.Cursor;
using QuanLib.Minecraft.NBT.Models;
using MCBS.Rendering;
using MCBS.Directorys;
using QuanLib.IO;
using Newtonsoft.Json;
using QuanLib.TickLoop;
using QuanLib.TickLoop.StateMachine;
using QuanLib.Logging;
using MCBS.Rendering.Extensions;
using MCBS.UI.Extensions;

namespace MCBS.Screens
{
    /// <summary>
    /// 屏幕运行时上下文
    /// </summary>
    public class ScreenContext : ITickUpdatable
    {
        private static readonly LogImpl LOGGER = LogManager.Instance.GetLogger();

        internal ScreenContext(Screen screen, IScreenView form, Guid guid = default)
        {
            ArgumentNullException.ThrowIfNull(screen, nameof(screen));
            ArgumentNullException.ThrowIfNull(form, nameof(form));

            Screen = screen;
            ScreenView = form;
            ScreenInputHandler = new(this);
            ScreenOutputHandler = new(this);
            IsRestarting = false;

            GUID = guid != default ? guid : Guid.NewGuid();
            StateMachine = new(ScreenState.NotLoaded, new StateContext<ScreenState>[]
            {
                new(ScreenState.NotLoaded, Array.Empty<ScreenState>(), GotoNotLoadedState),
                new(ScreenState.Active, new ScreenState[] { ScreenState.NotLoaded }, GotoActiveState, ActiveStateUpdate),
                new(ScreenState.Sleep, new ScreenState[] { ScreenState.Active }, GotoSleepState),
                new(ScreenState.Unload, new ScreenState[] { ScreenState.Active, ScreenState.Sleep }, GotoUnloadState)
            });

            _frame = null;
            _activeCursors = new();
            _offlineCursors = new();
        }

        private BlockFrame? _frame;

        private readonly List<CursorContext> _activeCursors;

        private readonly List<CursorContext> _offlineCursors;

        public Guid GUID { get; }

        public TickStateMachine<ScreenState> StateMachine { get; }

        public ScreenState ScreenState => StateMachine.CurrentState;

        public Screen Screen { get; }

        public IScreenView ScreenView { get; }

        public IRootForm RootForm => ScreenView.RootForm;

        public ScreenInputHandler ScreenInputHandler { get; }

        public ScreenOutputHandler ScreenOutputHandler { get; }

        public bool IsRestarting { get; private set; }

        protected virtual bool GotoNotLoadedState(ScreenState sourceState, ScreenState targetState)
        {
            return false;
        }

        protected virtual bool GotoActiveState(ScreenState sourceState, ScreenState targetState)
        {
            switch (sourceState)
            {
                case ScreenState.NotLoaded:
                    IsRestarting = false;
                    ScreenView.ClientSize = new(Screen.Width, Screen.Height);
                    ScreenView.HandleBeforeInitialize();
                    ScreenView.HandleInitialize();
                    ScreenView.HandleAfterInitialize();
                    foreach (var appId in SystemConfig.StartupChecklist)
                        MinecraftBlockScreen.Instance.ProcessManager.StartProcess(appId, RootForm);
                    SaveJson();
                    LOGGER.Info($"屏幕({Screen.StartPosition})已加载");
                    return true;
                case ScreenState.Sleep:
                    //TODO
                    return false;
                default:
                    return false;
            }
        }

        protected virtual bool GotoSleepState(ScreenState sourceState, ScreenState targetState)
        {
            //TODO
            return false;
        }

        protected virtual bool GotoUnloadState(ScreenState sourceState, ScreenState targetState)
        {
            foreach (var forem in MinecraftBlockScreen.Instance.FormManager.Items.Values)
            {
                if (forem.RootForm == RootForm)
                    forem.CloseForm();
            }
            RootForm.CloseForm();
            ScreenOutputHandler.FillAirBlock();
            DeleteJson();
            LOGGER.Info($"屏幕({Screen.StartPosition})已卸载");
            return true;
        }

        protected virtual void ActiveStateUpdate(int tick)
        {
            SaveJson();
        }

        public void OnTickUpdate(int tick)
        {
            StateMachine.OnTickUpdate(tick);
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
            StateMachine.Submit(ScreenState.Active);
            return this;
        }

        public void UnloadScreen()
        {
            StateMachine.Submit(ScreenState.Unload);
        }

        public void RestartScreen()
        {
            StateMachine.Submit(ScreenState.Unload);
            IsRestarting = true;
        }

        public void StartSleep()
        {
            StateMachine.Submit(ScreenState.Sleep);
        }

        public void StopSleep()
        {
            StateMachine.Submit(ScreenState.Active);
        }

        public Screen GetSubScreen()
        {
            return Screen.SubScreen(RootForm.GetRectangle());
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
            if (cursorContext.TextEditor.SynchronizeTick != MinecraftBlockScreen.Instance.SystemTick && oldData.TextEditor != newData.TextEditor)
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

        private void SaveJson()
        {
            MinecraftBlockScreen.Instance.FileWriteQueue.Submit(new TextWriteTask(GetSavePath(), ToJson()));
        }

        private void DeleteJson()
        {
            string savePath = GetSavePath();
            if (File.Exists(savePath))
                File.Delete(savePath);
        }

        private string ToJson()
        {
            Screen screen = GetSubScreen();
            Screen.DataModel model = screen.ToDataModel();
            return JsonConvert.SerializeObject(model);
        }

        private string GetSavePath()
        {
            return MinecraftBlockScreen.Instance.WorldDirectory.GetMcbsDataDirectory().ScreensDir.Combine(GUID + ".json");
        }
    }
}
