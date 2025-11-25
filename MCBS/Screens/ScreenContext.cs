using MCBS.Cursor;
using MCBS.ObjectModel;
using MCBS.Screens.Drawing;
using MCBS.UI;
using MCBS.UI.Extensions;
using Newtonsoft.Json;
using QuanLib.Core;
using QuanLib.IO;
using QuanLib.IO.Extensions;
using QuanLib.Logging;
using QuanLib.Minecraft.NBT.Models;
using QuanLib.TickLoop;
using QuanLib.TickLoop.StateMachine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MCBS.Config.ConfigManager;

namespace MCBS.Screens
{
    /// <summary>
    /// 屏幕运行时上下文
    /// </summary>
    public class ScreenContext : UnmanagedBase, IUnique, ITickUpdatable
    {
        private static readonly ILogger LOGGER = Log4NetManager.Instance.GetLogger();

        internal ScreenContext(Screen screen, IScreenView screenView, Guid guid)
        {
            ArgumentNullException.ThrowIfNull(screen, nameof(screen));
            ArgumentNullException.ThrowIfNull(screenView, nameof(screenView));

            if (guid == default)
                throw new ArgumentException("无效的GUID", nameof(guid));

            ScreenView = screenView;
            ScreenController = new(this, screen, 0, 1);
            ScreenInputHandler = new(this);
            ScreenDrawingHandler = new(this);
            ScreenUpdateHandler = new(this);
            IsRestarting = false;

            Guid = guid;
            StateMachine = new(ScreenState.NotLoaded, new StateContext<ScreenState>[]
            {
                new(ScreenState.NotLoaded, Array.Empty<ScreenState>(), GotoNotLoadedState),
                new(ScreenState.Active, new ScreenState[] { ScreenState.NotLoaded }, GotoActiveState, ActiveStateUpdate),
                new(ScreenState.Sleep, new ScreenState[] { ScreenState.Active }, GotoSleepState),
                new(ScreenState.Unload, new ScreenState[] { ScreenState.Active, ScreenState.Sleep }, GotoUnloadState)
            });

            _drawingContext = null;
            _activeCursors = [];
            _offlineCursors = [];

            DirectoryInfo? worldDirectory = MinecraftBlockScreen.Instance.MinecraftInstance.MinecraftPathManager.GetActiveWorlds().FirstOrDefault() ?? throw new InvalidOperationException("无法定位存档文件夹");
            McbsDataPathManager mcbsDataPathManager = McbsDataPathManager.FromWorldDirectoryCreate(worldDirectory.FullName);
            _savePath = mcbsDataPathManager.McbsData_Screens.FullName.PathCombine(Guid + ".json");
        }

        private ScreenDrawingContext? _drawingContext;

        private readonly List<CursorContext> _activeCursors;

        private readonly List<CursorContext> _offlineCursors;

        private readonly string _savePath;

        public Guid Guid { get; }

        public string ShortId => Guid.GetShortId();

        public TickStateMachine<ScreenState> StateMachine { get; }

        public ScreenState ScreenState => StateMachine.CurrentState;

        public Screen Screen => ScreenController.GetScreen();

        public IScreenView ScreenView { get; }

        public IRootForm RootForm => ScreenView.RootForm;

        public ScreenInputHandler ScreenInputHandler { get; }

        public ScreenDrawingHandler ScreenDrawingHandler { get; }

        public ScreenUpdateHandler ScreenUpdateHandler { get; }

        public ScreenController ScreenController { get; }

        public int MaxBackLayers => ScreenController.GetMaxBackLayers();

        public int MaxFrontLayers => ScreenController.GetMaxFrontLayers();

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
                    LOGGER.Info($"屏幕({ShortId})已加载");
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
            foreach (var forem in MinecraftBlockScreen.Instance.FormManager.Collection.GetForms())
            {
                if (forem.RootForm == RootForm)
                    forem.CloseForm();
            }
            RootForm.CloseForm();
            ScreenController.ClearScreenRange();
            DeleteJson();
            LOGGER.Info($"屏幕({ShortId})已卸载");
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

        public async Task HandleScreenControlAsync()
        {
            await Task.Run(() => ScreenController.HandleScreenControl());
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

        public async Task HandleFrameDrawingAsync()
        {
            _drawingContext = await ScreenDrawingHandler.HandleFrameDrawingAsync();
        }

        public async Task HandleFrameUpdateAsync()
        {
            if (_drawingContext is null)
                return;

            await ScreenUpdateHandler.HandleFrameUpdateAsync(_drawingContext.BaseLayer, 0);
            await ScreenUpdateHandler.HandleFrameUpdateAsync(_drawingContext.CursorLayer, 1);
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

        public CursorContext[] GetCursors()
        {
            return _activeCursors.ToArray();
        }

        public Screen GetSubScreen()
        {
            return Screen.SubScreen(RootForm.GetRectangle());
        }

        public override string ToString()
        {
            Screen subScreen = GetSubScreen();
            return $"Id={ShortId} State={StateMachine.CurrentState} Position={subScreen.StartPosition} Size=({subScreen.Width},{subScreen.Height}) Facing={subScreen.NormalFacing}";
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
            if (deputyItem == ScreenConfig.RightClickItemId || deputyItem == ScreenConfig.TextEditorItemId)
            {
                if (oldData.InventorySlot != newData.InventorySlot)
                    ScreenView.HandleCursorSlotChanged(new(newData.CursorPosition, cursorContext));
                if (!Item.EqualsID(oldData.DeputyItem, newData.DeputyItem))
                    ScreenView.HandleCursorItemChanged(new(newData.CursorPosition, cursorContext));
            }
        }

        private void SaveJson()
        {
            MinecraftBlockScreen.Instance.FileWriteQueue.Submit(new TextWriteTask(_savePath, ToJson()));
        }

        private void DeleteJson()
        {
            if (File.Exists(_savePath))
                File.Delete(_savePath);
        }

        private string ToJson()
        {
            Screen.DataModel model = Screen.ToDataModel();
            return JsonConvert.SerializeObject(model);
        }

        protected override void DisposeUnmanaged()
        {
            if (ScreenState != ScreenState.Active && ScreenState != ScreenState.Sleep)
                return;

            if (!MinecraftBlockScreen.Instance.MinecraftInstance.TestConnectivity())
                return;

            ScreenController.ClearScreenRange();
            LOGGER.Info($"屏幕({ShortId})已清除");
        }
    }
}
