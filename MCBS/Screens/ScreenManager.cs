using static MCBS.Config.ConfigManager;
using QuanLib.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MCBS.UI;
using Newtonsoft.Json;
using QuanLib.TickLoop;
using QuanLib.Logging;
using QuanLib.IO.Extensions;
using QuanLib.Core.Events;

namespace MCBS.Screens
{
    public partial class ScreenManager : UnmanagedBase, ITickUpdatable
    {
        private static readonly ILogger LOGGER = Log4NetManager.Instance.GetLogger();

        public ScreenManager()
        {
            Collection = new(this);
            ScreenOutputHandler = new(this);

            AddedScreen += OnAddedScreen;
            RemovedScreen += OnRemovedScreen;
        }

        public ScreenCollection Collection { get; }

        public ScreenOutputHandler ScreenOutputHandler { get; }

        public event EventHandler<ScreenManager, EventArgs<ScreenContext>> AddedScreen;

        public event EventHandler<ScreenManager, EventArgs<ScreenContext>> RemovedScreen;

        protected virtual void OnAddedScreen(ScreenManager sender, EventArgs<ScreenContext> e) { }

        protected virtual void OnRemovedScreen(ScreenManager sender, EventArgs<ScreenContext> e) { }

        public void Initialize()
        {
            DirectoryInfo? worldDirectory = MinecraftBlockScreen.Instance.MinecraftInstance.MinecraftPathManager.GetActiveWorlds().FirstOrDefault();

            if (worldDirectory is null)
                return;

            McbsDataPathManager mcbsDataPathManager = McbsDataPathManager.FromWorldDirectoryCreate(worldDirectory.FullName);
            mcbsDataPathManager.McbsData_Screens.CreateIfNotExists();
            string[] files = mcbsDataPathManager.McbsData_Screens.GetFilePaths("*.json");

            foreach (string file in files)
            {
                try
                {
                    string json = File.ReadAllText(file);
                    Screen.DataModel model = JsonConvert.DeserializeObject<Screen.DataModel>(json) ?? throw new FormatException();
                    Screen screen = Screen.FromDataModel(model);
                    string name = Path.GetFileNameWithoutExtension(file);
                    Guid guid = Guid.Parse(name);
                    ScreenContext screenContext = MinecraftBlockScreen.Instance.BuildScreen(screen, guid);

                    LOGGER.Info($"成功从文件“{Path.GetFileName(file)}”构建屏幕({screenContext.ShortId})");
                }
                catch (Exception ex)
                {
                    LOGGER.Error($"无法从文件“{Path.GetFileName(file)}”构建屏幕", ex);
                }
            }
        }

        public void OnTickUpdate(int tick)
        {
            foreach (ScreenContext screenContext in Collection.GetScreens())
            {
                var stateMachine = screenContext.StateMachine;

                if (stateMachine.CurrentState == ScreenState.Active)
                {
                    if (ScreenConfig.ScreenIdleTimeout != -1 && screenContext.ScreenInputHandler.IdleTime >= ScreenConfig.ScreenIdleTimeout)
                    {
                        screenContext.UnloadScreen();
                        LOGGER.Warn($"屏幕({screenContext.ShortId})已达到最大闲置时间，即将卸载");
                    }
                }

                screenContext.OnTickUpdate(tick);

                if (stateMachine.CurrentState == ScreenState.Unload)
                {
                    Guid guid = screenContext.Guid;

                    lock (Collection)
                        Collection.RemoveScreen(guid);

                    if (screenContext.IsRestarting)
                        MinecraftBlockScreen.Instance.BuildScreen(screenContext.Screen, guid);
                }
            }
        }

        public ScreenContext LoadScreen(Screen screen, IScreenView screenView, Guid guid = default)
        {
            ArgumentNullException.ThrowIfNull(screen, nameof(screen));
            ArgumentNullException.ThrowIfNull(screenView, nameof(screenView));

            lock (Collection)
            {
                if (guid == default)
                    guid = Collection.PreGenerateGuid();

                ScreenContext screenContext = new(screen, screenView, guid);
                Collection.AddScreen(screenContext);
                screenContext.LoadScreen();
                return screenContext;
            }
        }

        public void HandleAllScreenControl()
        {
            List<Task> tasks = [];
            foreach (ScreenContext screenContext in Collection)
                tasks.Add(screenContext.HandleScreenControlAsync());
            Task.WaitAll(tasks.ToArray());
        }

        public void HandleAllScreenInput()
        {
            List<Task> tasks = [];
            foreach (ScreenContext screenContext in Collection)
                tasks.Add(screenContext.HandleScreenInputAsync());
            Task.WaitAll(tasks.ToArray());
        }

        public void HandleAllScreenEvent()
        {
            List<Task> tasks = [];
            foreach (ScreenContext screenContext in Collection)
                tasks.Add(screenContext.HandleScreenEventAsync());
            Task.WaitAll(tasks.ToArray());
        }

        public void HandleAllBeforeFrame()
        {
            List<Task> tasks = [];
            foreach (ScreenContext screenContext in Collection)
                tasks.Add(screenContext.HandleBeforeFrameAsync());
            Task.WaitAll(tasks.ToArray());
        }

        public void HandleAllFrameDrawing()
        {
            List<Task> tasks = [];
            foreach (ScreenContext screenContext in Collection.Where(w => w.StateMachine.CurrentState == ScreenState.Active))
                tasks.Add(screenContext.HandleFrameDrawingAsync());
            Task.WaitAll(tasks.ToArray());
        }

        public void HandleAllFrameUpdate()
        {
            List<Task> tasks = [];
            foreach (ScreenContext screenContext in Collection.Where(w => w.StateMachine.CurrentState == ScreenState.Active))
                tasks.Add(screenContext.HandleFrameUpdateAsync());
            Task.WaitAll(tasks.ToArray());
        }

        public async Task HandleAllScreenOutputAsync()
        {
            await ScreenOutputHandler.HandleOutputAsync();
        }

        public void HandleAllAfterFrame()
        {
            List<Task> tasks = [];
            foreach (ScreenContext screenContext in Collection)
                tasks.Add(screenContext.HandleAfterFrameAsync());
            Task.WaitAll(tasks.ToArray());
        }

        protected override void DisposeUnmanaged()
        {
            ScreenContext[] screens = Collection.GetScreens();
            List<Exception>? exceptions = null;

            foreach (ScreenContext screenContext in screens)
            {
                try
                {
                    screenContext.Dispose();
                }
                catch (Exception ex)
                {
                    exceptions ??= [];
                    exceptions.Add(ex);
                }
            }

            try
            {
                Collection.ClearAllScreen();
            }
            catch (AggregateException ex) when (exceptions is not null)
            {
                exceptions.Add(ex);
            }

            if (exceptions is not null && exceptions.Count > 0)
                throw new AggregateException(exceptions);
        }
    }
}
