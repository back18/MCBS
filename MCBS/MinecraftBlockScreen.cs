﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net.Core;
using QuanLib.Core;
using QuanLib.Minecraft.Instance;
using MCBS.Screens;
using MCBS.UI;
using MCBS.Config;
using MCBS.Processes;
using MCBS.Application;
using MCBS.Forms;
using MCBS.Interaction;
using MCBS.Cursor;
using MCBS.Scoreboard;
using MCBS.Screens.Building;
using QuanLib.Minecraft.Command;
using QuanLib.IO;
using QuanLib.Minecraft.Command.Models;
using QuanLib.TickLoop;
using QuanLib.Logging;
using QuanLib.IO.Extensions;
using MCBS.Analyzer;
using QuanLib.Clipping;
using MCBS.Events;

namespace MCBS
{
    public class MinecraftBlockScreen : TickLoopSystem, ISingleton<MinecraftBlockScreen, MinecraftBlockScreen.InstantiateArgs>
    {
        private static readonly LogImpl LOGGER = LogManager.Instance.GetLogger();

        private MinecraftBlockScreen(MinecraftInstance minecraftInstance, ApplicationManifest[] appComponents) : base(TimeSpan.FromMilliseconds(50), LogManager.Instance.LoggerGetter)
        {
            ArgumentNullException.ThrowIfNull(minecraftInstance, nameof(minecraftInstance));
            ArgumentNullException.ThrowIfNull(appComponents, nameof(appComponents));

            MinecraftInstance = minecraftInstance;
            Clipboard = new();
            FileWriteQueue = new();
            MsptAnalyzer = new();
            TaskManager = new();
            ScreenManager = new();
            ProcessManager = new();
            FormManager = new();
            ScreenBuildManager = new();
            InteractionManager = new();
            ScoreboardManager = new();
            CursorManager = new();
            AppComponents = new(appComponents.ToDictionary(item => item.ID, item => item));

            GameTick = 0;
            SystemStage = SystemStage.ScreenScheduling;

            CreateAppComponentsDirectory();

            SystemStageSatrt += OnSystemStageSatrt;
            SystemStageEnd += OnSystemStageEnd;

            _msptRecord = new(this);
            _query = Task.Run(() => 0);
        }

        private static readonly object _slock = new();

        public static bool IsInstanceLoaded => _Instance is not null;

        public static MinecraftBlockScreen Instance => _Instance ?? throw new InvalidOperationException("实例未加载");
        private static MinecraftBlockScreen? _Instance;

        private readonly SystemStageRecord _msptRecord;

        private Task<int> _query;

        public int GameTick { get; private set; }

        public SystemStage SystemStage { get; private set; }

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

        private void ConnectMinecraft()
        {
            LOGGER.Info($"正在等待位于“{MinecraftInstance.MinecraftPathManager.Minecraft.FullName}”的Minecraft实例启动...");
            MinecraftInstance.WaitForConnection();
            MinecraftInstance.Start("MinecraftInstance Thread");
            Thread.Sleep(1000);

            LOGGER.Info("成功连接到Minecraft实例");
        }

        private void Initialize()
        {
            LOGGER.Info("MCBS开始初始化");

            _query = GetGameTickAsync();
            GameTick = _query.Result;
            TaskManager.Initialize();
            ScreenManager.Initialize();
            InteractionManager.Initialize();
            ScoreboardManager.Initialize();
            FileWriteQueue.Start("FileWrite Thread");

            LOGGER.Info("MCBS初始化完成");
        }

        protected override void OnTickStart(int tick)
        {
            _msptRecord.Reset();
        }

        public override void OnTickUpdate(int tick)
        {
            ScreenScheduling(tick);
            ProcessScheduling(tick);
            FormScheduling(tick);

            if (_query.IsCompleted)
            {
                CommandManager.SnbtCache.Clear();
                InteractionScheduling(tick);
                ScoreboardScheduling(tick);
                ScreenBuildScheduling(tick);
                HandleScreenInput(tick);
                HandleScreenEvent(tick);
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
            ConnectMinecraft();
            Initialize();

            LOGGER.Info("MCBS已开始运行");

            run:

            try
            {
                base.Run();
            }
            catch (Exception ex)
            {
                bool connection = MinecraftInstance.TestConnectivity();

                if (!connection)
                {
                    LOGGER.Fatal("MCBS运行时引发了异常，并且与Minecraft实例断开了连接，系统即将终止", ex);
                }
                else if (ConfigManager.SystemConfig.CrashAutoRestart)
                {
                    foreach (var context in ScreenManager.Items.Values)
                    {
                        context.RestartScreen();
                    }
                    LOGGER.Error("MCBS运行时引发了异常，已启用自动重启，系统即将在3秒后重启", ex);
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
                    LOGGER.Fatal("MCBS运行时引发了异常，并且未启用自动重启，系统即将终止", ex);
                }
            }

            LOGGER.Info("MCBS已终止运行");
        }

        protected override void DisposeUnmanaged()
        {
            try
            {
                LOGGER.Info("开始释放系统资源");

                LOGGER.Info("正在等待主任务完成...");
                TaskManager.WaitForCurrentMainTask();

                if (IsRunning)
                {
                    StopSubTasks();
                    Task task = WaitForStopAsync();
                    LOGGER.Info("正在等待MCBS终止运行...");
                    IsRunning = false;
                    task.Wait();
                }

                bool connection = MinecraftInstance.TestConnectivity();
                if (!connection)
                {
                    LOGGER.Warn("由于无法继续连接到Minecraft实例，部分系统资源可能无法回收");
                    goto end;
                }

                if (ScreenManager.Items.Count > 0)
                {
                    LOGGER.Info($"即将卸载所有屏幕，共计{ScreenManager.Items.Count}个");
                    foreach (var context in ScreenManager.Items.Values)
                        context.UnloadScreen();
                }
               
                if (InteractionManager.Items.Count > 0)
                {
                    LOGGER.Info($"即将回收所有交互实体，共计{InteractionManager.Items.Count}个");
                    foreach (var interaction in InteractionManager.Items.Values)
                        interaction.CloseInteraction();
                }

                int tick = SystemTick + 1;
                ScreenScheduling(tick);
                ProcessScheduling(tick);
                FormScheduling(tick);
                InteractionScheduling(tick);
                Thread.Sleep(1000);

                LOGGER.Info("系统资源均已释放完成");
            }
            catch (Exception ex)
            {
                LOGGER.Error("释放系统资源时引发了异常", ex);
            }

            end:
            FileWriteQueue.Stop();
            MinecraftInstance.Stop();
            LOGGER.Info("已和Minecraft实例断开连接");
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

            TaskManager.ResetCurrentMainTask();
            Task task = ScreenManager.HandleAllScreenOutputAsync();
            TaskManager.SetCurrentMainTask(task);
            TaskManager.WaitForPreviousMainTask();
            GameTick = _query.Result;
            _query = GetGameTickAsync();

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
            foreach (var appId in AppComponents.Keys)
                McbsPathManager.MCBS_Applications.CombineDirectory(appId).CreateIfNotExists();
        }

        public async Task<int> GetGameTickAsync()
        {
            TimeQueryGametimeCommand command = CommandManager.TimeQueryGametimeCommand;

            if (!command.Input.TryFormat([], out var input))
                return 0;

            string output = await MinecraftInstance.CommandSender.SendCommandAsync(input);

            if (!command.Output.TryMatch(output, out var outargs))
                return 0;

            if (outargs is null || outargs.Length != 1 || !int.TryParse(outargs[0], out var result))
                return 0;

            return result;
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
