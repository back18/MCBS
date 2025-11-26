using MCBS.Analyzer;
using MCBS.Application;
using MCBS.Cursor;
using MCBS.Events;
using MCBS.Forms;
using MCBS.Interaction;
using MCBS.Processes;
using MCBS.Scoreboard;
using MCBS.Screens;
using MCBS.Screens.Building;
using MCBS.UI;
using QuanLib.Clipping;
using QuanLib.Core;
using QuanLib.IO;
using QuanLib.IO.Extensions;
using QuanLib.Logging;
using QuanLib.Minecraft.Command;
using QuanLib.Minecraft.Instance;
using QuanLib.TickLoop;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MCBS
{
    public class MinecraftBlockScreen : TickLoopSystem, ISingleton<MinecraftBlockScreen, MinecraftBlockScreen.InstantiateArgs>
    {
        private MinecraftBlockScreen(MinecraftInstance minecraftInstance, ApplicationManifest[] appComponents) : base(TimeSpan.FromMilliseconds(50), Log4NetManager.Instance.GetProvider())
        {
            ArgumentNullException.ThrowIfNull(minecraftInstance, nameof(minecraftInstance));
            ArgumentNullException.ThrowIfNull(appComponents, nameof(appComponents));

            MinecraftInstance = minecraftInstance;
            Clipboard = new();
            MsptAnalyzer = new();
            TaskManager = new();
            ScreenManager = new();
            ProcessManager = new();
            FormManager = new();
            ScreenBuildManager = new();
            InteractionManager = new();
            ScoreboardManager = new();
            CursorManager = new();
            AppComponents = new(appComponents.ToDictionary(item => item.Id, item => item));

            GameTick = -1;
            SystemStage = SystemStage.ScreenScheduling;
            TimestampForCommandHandle = SystemRunningTime;
            MaxDelayForCommandHandle = TimeSpan.FromSeconds(1);

            FileWriteQueue = new(Log4NetManager.Instance.GetProvider());
            FileWriteQueue.SetDefaultThreadName("FileWrite Thread");
            AddSubtask(FileWriteQueue);

            CreateAppComponentsDirectory();

            SystemStageSatrt += OnSystemStageSatrt;
            SystemStageEnd += OnSystemStageEnd;

            _msptRecord = new(this);
            _gtQuery = Task.FromResult(-1);
        }

        private static readonly object _slock = new();

        public static bool IsInstanceLoaded => _Instance is not null;

        public static MinecraftBlockScreen Instance => _Instance ?? throw new InvalidOperationException("实例未加载");
        private static MinecraftBlockScreen? _Instance;

        private readonly SystemStageRecord _msptRecord;

        private Task<int> _gtQuery;

        public int GameTick { get; private set; }

        public SystemStage SystemStage { get; private set; }

        public TimeSpan TimestampForCommandHandle { get; private set; }

        public TimeSpan MaxDelayForCommandHandle { get; }

        public MinecraftInstance MinecraftInstance { get; }

        public Clipboard Clipboard { get; }

        public FileWriteQueue FileWriteQueue { get; }

        public MsptAnalyzer<SystemStage> MsptAnalyzer { get; }

        public TaskManager TaskManager { get; }

        public ScreenManager ScreenManager { get; }

        public ProcessManager ProcessManager { get; }

        public FormManager FormManager { get; }

        public InteractionManager InteractionManager { get; }

        public ScoreboardManager ScoreboardManager { get; }

        public ScreenBuildManager ScreenBuildManager { get; }

        public CursorManager CursorManager { get; }

        public ReadOnlyDictionary<string, ApplicationManifest> AppComponents { get; }

        public event ValueEventHandler<MinecraftBlockScreen, TickStageEventArgs> SystemStageSatrt;

        public event ValueEventHandler<MinecraftBlockScreen, TickStageEventArgs> SystemStageEnd;

        protected virtual void OnSystemStageSatrt(MinecraftBlockScreen sender, TickStageEventArgs e) { }

        protected virtual void OnSystemStageEnd(MinecraftBlockScreen sender, TickStageEventArgs e) { }

        public static MinecraftBlockScreen LoadInstance(InstantiateArgs instantiateArgs)
        {
            ArgumentNullException.ThrowIfNull(instantiateArgs, nameof(instantiateArgs));

            lock (_slock)
            {
                if (_Instance is not null)
                    throw new InvalidOperationException("试图重复加载单例实例");

                _Instance = new(instantiateArgs.MinecraftInstance, instantiateArgs.AppComponents);
                return _Instance;
            }
        }

        private void Initialize()
        {
            Logger?.Info("MCBS开始初始化");

            QueryGameTick();
            UpdateGameTick();
            TaskManager.Initialize();
            ScreenManager.Initialize();
            InteractionManager.Initialize();
            ScoreboardManager.Initialize();

            Logger?.Info("MCBS初始化完成");
        }

        protected override void OnTickStart(int tick)
        {
            _msptRecord.Reset();
        }

        public override void OnTickUpdate(int tick)
        {
            if (!MinecraftInstance.IsRunning)
            {
                Logger?.Warn($"Minecraft实例已断开连接，当前Tick({SystemTick})操作无法完成");
                return;
            }

            ScreenScheduling(tick);
            ProcessScheduling(tick);
            FormScheduling(tick);

            bool isCompleted = TaskManager.IsCompletedMainTask;
            TimeSpan delay = SystemRunningTime - TimestampForCommandHandle;
            if (!isCompleted && delay > MaxDelayForCommandHandle)
            {
                Logger?.Warn($"命令总线已持续繁忙超过{(int)delay.TotalMilliseconds}ms，将强制等待处理命令");
                TaskManager.WaitForMainTask();
                isCompleted = true;
            }

            if (isCompleted)
            {
                CommandManager.SnbtCache.Clear();
                InteractionScheduling(tick);
                ScoreboardScheduling(tick);
                ScreenBuildScheduling(tick);
                HandleScreenControl(tick);
                HandleScreenInput(tick);
                HandleScreenEvent(tick);
                TimestampForCommandHandle = SystemRunningTime;
            }
            else
            {
                Logger?.Debug($"命令总线繁忙，当前Tick({SystemTick})已跳过命令处理");
            }

            HandleBeforeFrame(tick);
            HandleFrameDrawing(tick);
            HandleFrameUpdate(tick);
            HandleScreenOutput(tick);
            HandleAfterFrame(tick);
        }

        protected override void OnTickEnd(int tick)
        {
            _msptRecord.Stop();
            MsptAnalyzer.Update(_msptRecord);
        }

        protected override void Run()
        {
            if (!MinecraftInstance.TestConnectivity())
            {
                Logger?.Error("由于Minecraft实例未连接，系统无法启动");
                return;
            }

            Initialize();

            Logger?.Info("MCBS已开始运行");

            try
            {
                base.Run();
            }
            catch (ThreadInterruptedException ex)
            {
                Logger?.Warn("MCBS系统线程被外部强行中断，系统即将终止", ex);
            }
            catch (Exception ex)
            {
                Logger?.Fatal("MCBS运行时引发了异常，系统即将终止", ex);
            }

            Logger?.Info("MCBS系统正在停止...");

            IsRunning = false;
            DisposeMinecraftResource();

            Logger?.Info("MCBS已终止运行");
        }

        private void DisposeMinecraftResource()
        {
            if (!MinecraftInstance.TestConnectivity())
            {
                Logger?.Warn("提前与Minecraft实例断开连接，托管在Minecraft的资源可能无法回收");
                return;
            }

            try
            {
                FormManager.Dispose();
                ProcessManager.Dispose();
                ScreenManager.Dispose();
                InteractionManager.Dispose();

                Logger?.Info("Minecraft托管资源已完成回收");
            }
            catch (Exception ex)
            {
                Logger?.Error("释放托管在Minecraft的资源时引发了异常，部分资源可能无法回收", ex);
            }
        }

        public void RequestStop()
        {
            IsRunning = false;
        }

        private void ScreenScheduling(int tick)
        {
            SystemStage = SystemStage.ScreenScheduling;
            SystemStageSatrt.Invoke(this, new(tick, SystemStage.ScreenScheduling));

            ScreenManager.OnTickUpdate(tick);

            SystemStageEnd.Invoke(this, new(tick, SystemStage.ScreenScheduling));
        }

        private void ProcessScheduling(int tick)
        {
            SystemStage = SystemStage.ProcessScheduling;
            SystemStageSatrt.Invoke(this, new(tick, SystemStage.ProcessScheduling));

            ProcessManager.OnTickUpdate(tick);

            SystemStageEnd.Invoke(this, new(tick, SystemStage.ProcessScheduling));
        }

        private void FormScheduling(int tick)
        {
            SystemStage = SystemStage.FormScheduling;
            SystemStageSatrt.Invoke(this, new(tick, SystemStage.FormScheduling));

            FormManager.OnTickUpdate(tick);

            SystemStageEnd.Invoke(this, new(tick, SystemStage.FormScheduling));
        }

        private void InteractionScheduling(int tick)
        {
            SystemStage = SystemStage.InteractionScheduling;
            SystemStageSatrt.Invoke(this, new(tick, SystemStage.InteractionScheduling));

            InteractionManager.OnTickUpdate(tick);

            SystemStageEnd.Invoke(this, new(tick, SystemStage.InteractionScheduling));
        }

        private void ScoreboardScheduling(int tick)
        {
            SystemStage = SystemStage.ScoreboardScheduling;
            SystemStageSatrt.Invoke(this, new(tick, SystemStage.ScoreboardScheduling));

            ScoreboardManager.OnTickUpdate(tick);

            SystemStageEnd.Invoke(this, new(tick, SystemStage.ScoreboardScheduling));
        }

        private void ScreenBuildScheduling(int tick)
        {
            SystemStage = SystemStage.ScreenBuildScheduling;
            SystemStageSatrt.Invoke(this, new(tick, SystemStage.ScreenBuildScheduling));

            ScreenBuildManager.OnTickUpdate(tick);

            SystemStageEnd.Invoke(this, new(tick, SystemStage.ScreenBuildScheduling));
        }

        private void HandleScreenControl(int tick)
        {
            SystemStage = SystemStage.HandleScreenControl;
            SystemStageSatrt.Invoke(this, new(tick, SystemStage.HandleScreenControl));

            ScreenManager.HandleAllScreenControl();

            SystemStageEnd.Invoke(this, new(tick, SystemStage.HandleScreenControl));
        }

        private void HandleScreenInput(int tick)
        {
            SystemStage = SystemStage.HandleScreenInput;
            SystemStageSatrt.Invoke(this, new(tick, SystemStage.HandleScreenInput));

            ScreenManager.HandleAllScreenInput();

            SystemStageEnd.Invoke(this, new(tick, SystemStage.HandleScreenInput));
        }

        private void HandleScreenEvent(int tick)
        {
            SystemStage = SystemStage.HandleScreenEvent;
            SystemStageSatrt.Invoke(this, new(tick, SystemStage.HandleScreenEvent));

            ScreenManager.HandleAllScreenEvent();

            SystemStageEnd.Invoke(this, new(tick, SystemStage.HandleScreenEvent));
        }

        private void HandleBeforeFrame(int tick)
        {
            SystemStage = SystemStage.HandleBeforeFrame;
            SystemStageSatrt.Invoke(this, new(tick, SystemStage.HandleBeforeFrame));

            ScreenManager.HandleAllBeforeFrame();

            SystemStageEnd.Invoke(this, new(tick, SystemStage.HandleBeforeFrame));
        }

        private void HandleFrameDrawing(int tick)
        {
            SystemStage = SystemStage.HandleFrameDrawing;
            SystemStageSatrt.Invoke(this, new(tick, SystemStage.HandleFrameDrawing));

            ScreenManager.HandleAllFrameDrawing();

            SystemStageEnd.Invoke(this, new(tick, SystemStage.HandleFrameDrawing));
        }

        private void HandleFrameUpdate(int tick)
        {
            SystemStage = SystemStage.HandleFrameUpdate;
            SystemStageSatrt.Invoke(this, new(tick, SystemStage.HandleFrameUpdate));

            ScreenManager.HandleAllFrameUpdate();

            SystemStageEnd.Invoke(this, new(tick, SystemStage.HandleFrameUpdate));
        }

        private void HandleScreenOutput(int tick)
        {
            SystemStage = SystemStage.HandleScreenOutput;
            SystemStageSatrt.Invoke(this, new(tick, SystemStage.HandleScreenOutput));

            ScreenOutputHandler screenOutputHandler = ScreenManager.ScreenOutputHandler;

            if (TaskManager.IsCompletedMainTask)
            {
                QueryGameTick();
                TaskManager.SetMainTask(screenOutputHandler.HandleOutputAsync);
                UpdateGameTick();
            }
            else
            {
                while (screenOutputHandler.IsDelaying)
                    Thread.Yield();

                Task task = screenOutputHandler.HandleDelayOutputAsync();
                TaskManager.WaitForMainTask();
                QueryGameTick();

                int ms = (int)TaskManager.MainTaskRuntime.TotalMilliseconds;
                if (ms > TickMaxTime.TotalMilliseconds * 2)
                    Logger?.Warn($"当前Tick({SystemTick})命令总线处理速度严重滞后，总耗时{ms}ms");

                UpdateGameTick();
                screenOutputHandler.ReleaseSemaphore();
                TaskManager.SetMainTask(task);
            }

            SystemStageEnd.Invoke(this, new(tick, SystemStage.HandleScreenOutput));
        }

        public void HandleAfterFrame(int tick)
        {
            SystemStage = SystemStage.HandleAfterFrame;
            SystemStageSatrt.Invoke(this, new(tick, SystemStage.HandleAfterFrame));

            ScreenManager.HandleAllAfterFrame();

            SystemStageEnd.Invoke(this, new(tick, SystemStage.HandleAfterFrame));
        }

        public ScreenContext? ScreenContextOf(IForm form)
        {
            ArgumentNullException.ThrowIfNull(form, nameof(form));

            foreach (ScreenContext screenContext in ScreenManager.Collection.GetScreens())
            {
                if (screenContext.RootForm == form || screenContext.RootForm.ContainsForm(form))
                    return screenContext;
            }

            return null;
        }

        public ProcessContext? ProcessContextOf(IProgram program)
        {
            ArgumentNullException.ThrowIfNull(program, nameof(program));

            foreach (ProcessContext processContext in ProcessManager.Collection.GetProcesses())
            {
                if (program == processContext.Program)
                    return processContext;
            }

            return null;
        }

        public ProcessContext? ProcessContextOf(IForm form)
        {
            ArgumentNullException.ThrowIfNull(form, nameof(form));

            FormContext? formContext = FormContextOf(form);
            if (formContext is null)
                return null;

            return ProcessContextOf(formContext.Program);
        }

        public FormContext? FormContextOf(IForm form)
        {
            ArgumentNullException.ThrowIfNull(form, nameof(form));

            foreach (FormContext formContext in FormManager.Collection.GetForms())
            {
                if (form == formContext.Form)
                    return formContext;
            }

            return null;
        }

        public ScreenContext BuildScreen(Screen screen, Guid guid = default)
        {
            ProcessContext processContext = ProcessManager.StartServicesProcess();
            IScreenView screenView = ((IServicesProgram)processContext.Program).ScreenView;
            return ScreenManager.LoadScreen(screen, screenView, guid);
        }

        private async Task<int> GetGameTickAsync()
        {
            string outputPattern = CommandManager.TimeQueryGametimeCommand.Output.PatternText;
            string output;

            try
            {
                output = await MinecraftInstance.CommandSender.SendCommandAsync("time query gametime");
            }
            catch
            {
                return -1;
            }

            Match match = Regex.Match(output, outputPattern);
            if (!match.Success)
                return -1;

            string arg = match.Groups[1].Value;
            if (!int.TryParse(arg, out var result))
                return -1;

            return result;
        }

        private void QueryGameTick()
        {
            _gtQuery = GetGameTickAsync();
        }

        private void UpdateGameTick()
        {
            GameTick = _gtQuery.ConfigureAwait(false).GetAwaiter().GetResult();
        }

        private void CreateAppComponentsDirectory()
        {
            foreach (var appId in AppComponents.Keys)
                McbsPathManager.MCBS_Application.CombineDirectory(appId).CreateIfNotExists();
        }

        public class InstantiateArgs : QuanLib.Core.InstantiateArgs
        {
            public InstantiateArgs(MinecraftInstance minecraftInstance, ApplicationManifest[] appComponents)
            {
                ArgumentNullException.ThrowIfNull(minecraftInstance, nameof(minecraftInstance));
                ArgumentNullException.ThrowIfNull(appComponents, nameof(appComponents));

                MinecraftInstance = minecraftInstance;
                AppComponents = appComponents;
            }

            public MinecraftInstance MinecraftInstance { get; }

            public ApplicationManifest[] AppComponents { get; }
        }
    }
}
