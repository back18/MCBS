using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using QuanLib.BDF;
using SixLabors.ImageSharp;
using FFMediaToolkit;
using System.Collections.Concurrent;
using log4net.Core;
using System.Runtime.CompilerServices;
using QuanLib.Core;
using QuanLib.Minecraft.Instance;
using MCBS.Screens;
using MCBS.Logging;
using MCBS.UI;
using MCBS.Config;
using MCBS.Processes;
using MCBS.Application;
using MCBS.Forms;
using MCBS.Interaction;
using MCBS.Cursor;
using MCBS.RightClickObjective;
using MCBS.Screens.Building;
using System.Collections.ObjectModel;
using QuanLib.Minecraft.Command;
using QuanLib.IO;
using QuanLib.Minecraft.Directorys;

namespace MCBS
{
    public class MCOS : UnmanagedRunnable
    {
        private static readonly LogImpl LOGGER = LogUtil.GetLogger();

        static MCOS()
        {
            _slock = new();
            IsLoaded = false;
        }

        private MCOS(MinecraftInstance minecraftInstance, ApplicationManifest[] appComponents) : base(Logbuilder.Default)
        {
            ArgumentNullException.ThrowIfNull(minecraftInstance, nameof(minecraftInstance));
            ArgumentNullException.ThrowIfNull(appComponents, nameof(appComponents));

            MinecraftInstance = minecraftInstance;
            FileWriteQueue = new();
            TimeAnalysisManager = new();
            TaskManager = new();
            ScreenManager = new();
            ProcessManager = new();
            FormManager = new();
            ScreenBuildManager = new();
            InteractionManager = new();
            RightClickObjectiveManager = new();
            CursorManager = new();
            AppComponents = new(appComponents.ToDictionary(item => item.ID, item => item));

            PreviousTickTime = TimeSpan.Zero;
            NextTickTime = PreviousTickTime + TickTime;
            TickTime = TimeSpan.FromMilliseconds(50);
            GameTick = 0;
            SystemTick = 0;
            SystemStage = SystemStage.Other;

            _syatemStopwatch = new();
            _tickStopwatch = new();
            _times = new();
            _query = Task.Run(() => 0);

            CreateAppComponentsDirectory();
        }

        private static readonly object _slock;

        public static bool IsLoaded { get; private set; }

        public static MCOS Instance
        {
            get
            {
                if (_Instance is null)
                    throw new InvalidOperationException("实例未加载");
                return _Instance;
            }
        }
        private static MCOS? _Instance;

        private readonly Stopwatch _syatemStopwatch;

        private readonly Stopwatch _tickStopwatch;

        private readonly Dictionary<SystemStage, TimeSpan> _times;

        private Task<int> _query;

        public TimeSpan SystemRunningTime => _syatemStopwatch.Elapsed;

        public TimeSpan TickRunningTime => _tickStopwatch.Elapsed;

        public TimeSpan PreviousTickTime { get; private set; }

        public TimeSpan NextTickTime { get; private set; }

        public TimeSpan TickTime { get; }

        public int GameTick { get; private set; }

        public int SystemTick { get; private set; }

        public SystemStage SystemStage { get; private set; }

        public MinecraftInstance MinecraftInstance { get; }

        public WorldDirectory WorldDirectory => _WorldDirectory ?? throw new InvalidOperationException("当前状态无法定位世界文件夹");
        private WorldDirectory? _WorldDirectory;

        public FileWriteQueue FileWriteQueue { get; }

        public TimeAnalysisManager TimeAnalysisManager { get; }

        public TaskManager TaskManager { get; }

        public ScreenManager ScreenManager { get; }

        public ProcessManager ProcessManager { get; }

        public FormManager FormManager { get; }

        public InteractionManager InteractionManager { get; }

        public RightClickObjectiveManager RightClickObjectiveManager { get; }

        public ScreenBuildManager ScreenBuildManager { get; }

        public CursorManager CursorManager { get; }

        public ReadOnlyDictionary<string, ApplicationManifest> AppComponents { get; }

        public static MCOS LoadInstance(MinecraftInstance minecraftInstance, ApplicationManifest[] appComponents)
        {
            ArgumentNullException.ThrowIfNull(minecraftInstance, nameof(minecraftInstance));
            ArgumentNullException.ThrowIfNull(appComponents, nameof(appComponents));

            lock (_slock)
            {
                if (_Instance is not null)
                    throw new InvalidOperationException("试图重复加载单例实例");

                _Instance ??= new(minecraftInstance, appComponents);
                IsLoaded = true;
                return _Instance;
            }
        }

        private void Initialize()
        {
            LOGGER.Info("正在等待Minecraft实例启动...");

            while (true)
            {
                _WorldDirectory = MinecraftInstance.MinecraftDirectory.GetActiveWorldDirectory();
                if (_WorldDirectory is not null)
                    break;

                Thread.Sleep(1000);
            }

            MinecraftInstance.WaitForConnection();
            MinecraftInstance.Start("MinecraftInstance Thread");
            Thread.Sleep(1000);

            LOGGER.Info("成功连接到Minecraft实例");

            LOGGER.Info("开始初始化");

            _query = MinecraftInstance.GetGameTickAsync();
            GameTick = _query.Result;
            FileWriteQueue.Start("FileWrite Thread");
            TaskManager.Initialize();
            ScreenManager.Initialize();
            InteractionManager.Initialize();
            RightClickObjectiveManager.Initialize();

            LOGGER.Info("初始化完成");
        }

        protected override void Run()
        {
            LOGGER.Info("系统已启动");
            Initialize();
            _syatemStopwatch.Start();

            run:

            try
            {
                while (IsRunning)
                {
                    _tickStopwatch.Restart();
                    _times.Clear();
                    CommandManager.SnbtCache.Clear();
                    PreviousTickTime = SystemRunningTime;
                    NextTickTime = PreviousTickTime + TickTime;
                    SystemTick++;

                    ScreenScheduling();
                    ProcessScheduling();
                    FormScheduling();

                    if (_query.IsCompleted)
                    {
                        InteractionScheduling();
                        RightClickObjectiveScheduling();
                        ScreenBuildScheduling();
                        HandleScreenInput();
                        HandleScreenEvent();
                    }
                    else
                    {
                        //TaskManager.AddTempTask(() =>
                        //{
                        //    InteractionScheduling();
                        //    RightClickObjectiveScheduling();
                        //    ScreenBuildScheduling();
                        //    HandleScreenInput();
                        //});
                    }

                    HandleBeforeFrame();
                    HandleUIRendering();
                    HandleScreenOutput();
                    HandleAfterFrame();

                    HandleSystemInterrupt();

                    _tickStopwatch.Stop();
                    TimeAnalysisManager.Submit(_times, _tickStopwatch.Elapsed);
                }
            }
            catch (Exception ex)
            {
                //throw;

                bool connection = MinecraftInstance.TestConnectivity();

                if (!connection)
                {
                    LOGGER.Fatal("系统运行时引发了异常，并且无法连接到Minecraft实例，系统即将终止", ex);
                }
                else if (ConfigManager.SystemConfig.CrashAutoRestart)
                {
                    foreach (var context in ScreenManager.Items.Values)
                    {
                        context.RestartScreen();
                    }
                    LOGGER.Error("系统运行时引发了异常，已启用自动重启，系统即将在3秒后重启", ex);
                    for (int i = 3; i >= 1; i--)
                    {
                        LOGGER.Info($"将在{i}秒后自动重启...");
                        Thread.Sleep(1000);
                    }
                    TaskManager.Clear();
                    LOGGER.Info("开始重启...");
                    goto run;
                }
                else
                {
                    LOGGER.Fatal("系统运行时引发了异常，并且未启用自动重启，系统即将终止", ex);
                }
            }

            _syatemStopwatch.Stop();
            LOGGER.Info("系统已终止");
        }

        protected override void DisposeUnmanaged()
        {
            try
            {
                LOGGER.Info("开始回收系统资源");

                LOGGER.Info("正在等待主任务完成...");
                TaskManager.WaitForCurrentMainTask();

                if (IsRunning)
                {
                    Task task = WaitForStopAsync();
                    LOGGER.Info("正在等待系统终止...");
                    IsRunning = false;
                    task.Wait();
                }

                bool connection = MinecraftInstance.TestConnectivity();
                if (!connection)
                {
                    LOGGER.Warn("由于无法连接到Minecraft实例，部分系统资源可能无法回收");
                    goto end;
                }

                LOGGER.Info($"正在卸载所有屏幕，共计{ScreenManager.Items.Count}个");
                foreach (var context in ScreenManager.Items.Values)
                    context.UnloadScreen();
                LOGGER.Info("完成");

                LOGGER.Info($"正在回收交互实体，{InteractionManager.Items.Count}个");
                foreach (var interaction in InteractionManager.Items.Values)
                    interaction.CloseInteraction();
                LOGGER.Info("完成");

                ScreenScheduling();
                ProcessScheduling();
                FormScheduling();
                InteractionScheduling();
                Thread.Sleep(1000);

                LOGGER.Info("全部系统资源均已释放完成");
            }
            catch (Exception ex)
            {
                LOGGER.Error("无法回收系统资源", ex);
            }

            end:
            FileWriteQueue.Stop();
            MinecraftInstance.Stop();
            LOGGER.Info("已和Minecraft实例断开连接");
        }

        private void HandleAndTimeing(Action action, SystemStage stage)
        {
            ArgumentNullException.ThrowIfNull(action, nameof(action));

            SystemStage = stage;
            Stopwatch stopwatch = Stopwatch.StartNew();
            action.Invoke();
            stopwatch.Stop();
            SystemStage = SystemStage.Other;

            if (_times.TryGetValue(stage, out var time))
                _times[stage] = time + stopwatch.Elapsed;
            else
                _times.Add(stage, stopwatch.Elapsed);
        }

        private void ScreenScheduling()
        {
            HandleAndTimeing(ScreenManager.OnTick, SystemStage.ScreenScheduling);
        }

        private void ProcessScheduling()
        {
            HandleAndTimeing(ProcessManager.OnTick, SystemStage.ProcessScheduling);
        }

        private void FormScheduling()
        {
            HandleAndTimeing(FormManager.OnTick, SystemStage.FormScheduling);
        }

        private void InteractionScheduling()
        {
            HandleAndTimeing(InteractionManager.OnTick, SystemStage.InteractionScheduling);
        }

        private void RightClickObjectiveScheduling()
        {
            HandleAndTimeing(RightClickObjectiveManager.OnTick, SystemStage.RightClickObjectiveScheduling);
        }

        private void ScreenBuildScheduling()
        {
            HandleAndTimeing(ScreenBuildManager.OnTick, SystemStage.ScreenBuildScheduling);
        }

        private void HandleScreenInput()
        {
            HandleAndTimeing(ScreenManager.HandleAllScreenInput, SystemStage.HandleScreenInput);
        }

        private void HandleScreenEvent()
        {
            HandleAndTimeing(ScreenManager.HandleAllScreenEvent, SystemStage.HandleScreenEvent);
        }

        private void HandleBeforeFrame()
        {
            HandleAndTimeing(ScreenManager.HandleAllBeforeFrame, SystemStage.HandleBeforeFrame);
        }

        private void HandleUIRendering()
        {
            HandleAndTimeing(ScreenManager.HandleAllUIRendering, SystemStage.HandleUIRendering);
        }

        private void HandleScreenOutput()
        {
            HandleAndTimeing(() =>
            {
                TaskManager.ResetCurrentMainTask();
                Task task = ScreenManager.HandleAllScreenOutputAsync();
                TaskManager.SetCurrentMainTask(task);
                TaskManager.WaitForPreviousMainTask();
                GameTick = _query.Result;
                _query = MinecraftInstance.GetGameTickAsync();
            },
            SystemStage.HandleScreenOutput);
        }

        public void HandleAfterFrame()
        {
            HandleAndTimeing(ScreenManager.HandleAllAfterFrame, SystemStage.HandleAfterFrame);
        }

        private void HandleSystemInterrupt()
        {
            HandleAndTimeing(() =>
            {
                int time = (int)((NextTickTime - SystemRunningTime).TotalMilliseconds - 10);
                if (time > 0)
                    Thread.Sleep(time);
                while (SystemRunningTime < NextTickTime)
                    Thread.Yield();
            },
            SystemStage.HandleSystemInterrupt);
        }

        public ScreenContext? ScreenContextOf(IForm form)
        {
            ArgumentNullException.ThrowIfNull(form, nameof(form));

            foreach (var context in ScreenManager.Items.Values)
                if (context.RootForm == form || context.RootForm.ContainsForm(form))
                    return context;

            return null;
        }

        public ProcessContext? ProcessContextOf(IProgram program)
        {
            ArgumentNullException.ThrowIfNull(program, nameof(program));

            foreach (var context in ProcessManager.Items.Values)
                if (program == context.Program)
                    return context;

            return null;
        }

        public ProcessContext? ProcessContextOf(IForm form)
        {
            ArgumentNullException.ThrowIfNull(form, nameof(form));

            FormContext? context = FormContextOf(form);
            if (context is null)
                return null;

            return ProcessContextOf(context.Program);
        }

        public FormContext? FormContextOf(IForm form)
        {
            ArgumentNullException.ThrowIfNull(form, nameof(form));

            foreach (var context in FormManager.Items.Values.ToArray())
                if (form == context.Form)
                    return context;

            return null;
        }

        public ScreenContext BuildScreen(Screen screen, Guid guid = default)
        {
            ArgumentNullException.ThrowIfNull(screen, nameof(screen));

            ProcessContext processContext = ProcessManager.StartServicesProcess();
            IScreenView screenView = ((IServicesProgram)processContext.Program).ScreenView;
            return ScreenManager.LoadScreen(screen, screenView, guid);
        }

        private void CreateAppComponentsDirectory()
        {
            foreach (var appID in AppComponents.Keys)
            {
                string dir = Path.Combine(SR.McbsDirectory.ApplicationsDir.FullPath, appID);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
            }
        }
    }
}
