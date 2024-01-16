using static MCBS.Config.ConfigManager;
using log4net.Core;
using MCBS.Events;
using MCBS.Logging;
using QuanLib.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MCBS.Processes;
using MCBS.UI;
using Newtonsoft.Json;
using MCBS.Directorys;
using Microsoft.Extensions.Options;

namespace MCBS.Screens
{
    public partial class ScreenManager : ITickable
    {
        private static readonly LogImpl LOGGER = LogUtil.GetLogger();

        public ScreenManager()
        {
            Items = new(this);

            AddedScreen += OnAddedScreen;
            RemovedScreen += OnRemovedScreen;
        }

        public ScreenCollection Items { get; }

        public event EventHandler<ScreenManager, ScreenContextEventArgs> AddedScreen;

        public event EventHandler<ScreenManager, ScreenContextEventArgs> RemovedScreen;

        protected virtual void OnAddedScreen(ScreenManager sender, ScreenContextEventArgs e) { }

        protected virtual void OnRemovedScreen(ScreenManager sender, ScreenContextEventArgs e) { }

        public void Initialize()
        {
            ScreensDirectory directory = MinecraftBlockScreen.Instance.MinecraftInstance.MinecraftDirectory.GetActiveWorldDirectory()?.GetMcbsDataDirectory()?.ScreensDir ?? throw new InvalidOperationException("无法定位游戏存档文件夹");
            if (!directory.Exists())
                return;

            string[] files = directory.GetFiles("*.json");
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

                    LOGGER.Info($"成功从文件“{Path.GetFileName(file)}”构建屏幕({screenContext.Screen.StartPosition})");
                }
                catch (Exception ex)
                {
                    LOGGER.Error($"无法从文件“{Path.GetFileName(file)}”构建屏幕", ex);
                }
            }
        }

        public void OnTick()
        {
            foreach (var item in Items)
            {
                var state = item.Value.StateManager;

                if (state.CurrentState == ScreenState.Active)
                {
                    if (ScreenConfig.ScreenIdleTimeout != -1 && item.Value.ScreenInputHandler.IdleTime >= ScreenConfig.ScreenIdleTimeout)
                    {
                        item.Value.UnloadScreen();
                        LOGGER.Warn($"屏幕({item.Value.Screen.StartPosition})已达到最大闲置时间，即将卸载");
                    }
                }

                item.Value.OnTick();

                if (state.CurrentState == ScreenState.Unload)
                {
                    Items.TryRemove(item.Key, out _);
                    if (item.Value.IsRestarting)
                        MinecraftBlockScreen.Instance.BuildScreen(item.Value.GetSubScreen(), item.Key);
                }
            }
        }

        public ScreenContext LoadScreen(Screen screen, IScreenView screenView, Guid guid = default)
        {
            ArgumentNullException.ThrowIfNull(screen, nameof(screen));
            ArgumentNullException.ThrowIfNull(screenView, nameof(screenView));

            ScreenContext screenContext = new(screen, screenView, guid);
            if (!Items.TryAdd(screenContext.GUID, screenContext))
                throw new InvalidOperationException();

            screenContext.LoadScreen();
            return screenContext;
        }

        public void HandleAllScreenInput()
        {
            List<Task> tasks = new();
            foreach (var screenContext in Items.Values)
                tasks.Add(screenContext.HandleScreenInputAsync());
            Task.WaitAll(tasks.ToArray());
        }

        public void HandleAllScreenEvent()
        {
            List<Task> tasks = new();
            foreach (var screenContext in Items.Values)
                tasks.Add(screenContext.HandleScreenEventAsync());
            Task.WaitAll(tasks.ToArray());
        }

        public void HandleAllBeforeFrame()
        {
            List<Task> tasks = new();
            foreach (var screenContext in Items.Values)
                tasks.Add(screenContext.HandleBeforeFrameAsync());
            Task.WaitAll(tasks.ToArray());
        }

        public void HandleAllUIRendering()
        {
            List<Task> tasks = new();
            foreach (var screenContext in Items.Values)
                tasks.Add(screenContext.HandleUIRenderingAsync());
            Task.WaitAll(tasks.ToArray());
        }

        public async Task HandleAllScreenOutputAsync()
        {
            List<Task> tasks = new();
            foreach (var screenContext in Items.Values)
                tasks.Add(screenContext.HandleScreenOutputAsync());
            await Task.WhenAll(tasks);
        }

        public void HandleAllAfterFrame()
        {
            List<Task> tasks = new();
            foreach (var screenContext in Items.Values)
                tasks.Add(screenContext.HandleAfterFrameAsync());
            Task.WaitAll(tasks.ToArray());
        }
    }
}
