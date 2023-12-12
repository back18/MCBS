#define TryCatch

using static MCBS.Config.ConfigManager;
using log4net.Core;
using MCBS.Directorys;
using MCBS.Logging;
using MCBS.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QuanLib.Core;
using QuanLib.Minecraft.Directorys;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MCBS.Processes;
using MCBS.Events;
using MCBS.Application;
using MCBS.Cursor.Style;
using MCBS.Screens.Building;
using MCBS.Cursor;

namespace MCBS.Screens
{
    public class ScreenManager : ITickable
    {
        private static readonly LogImpl LOGGER = LogUtil.GetLogger();

        public ScreenManager()
        {
            Items = new(this);

            _saves = new();

            AddedScreen += OnAddedScreen;
            RemovedScreen += OnRemovedScreen;
        }

        private readonly List<ScreenOptions> _saves;

        public ScreenCollection Items { get; }

        public event EventHandler<ScreenManager, ScreenContextEventArgs> AddedScreen;

        public event EventHandler<ScreenManager, ScreenContextEventArgs> RemovedScreen;

        protected virtual void OnAddedScreen(ScreenManager sender, ScreenContextEventArgs e) { }

        protected virtual void OnRemovedScreen(ScreenManager sender, ScreenContextEventArgs e) { }

        public void Initialize()
        {
            LOGGER.Info($"开始加载常驻屏幕，共计{ScreenConfig.ResidentScreenList.Count}个");
            foreach (var options in ScreenConfig.ResidentScreenList)
            {
#if TryCatch
                try
                {
#endif
                    Items.Add(new(options)).LoadScreen();
#if TryCatch
                }
                catch (Exception ex)
                {
                    LOGGER.Error($"屏幕“{options}”无法加载", ex);
                }
#endif
            }

            ReadScreens();
            IEnumerable<ScreenOptions> creates = creates = _saves.Where(save => !Items.Values.Any(context => context.Screen.EqualsScreenOption(save)));

            LOGGER.Info($"开始加载上次未关闭屏幕，共计{creates.Count()}个");
            foreach (var options in creates)
            {
#if TryCatch
                try
                {
#endif
                    Items.Add(new(options)).LoadScreen();
#if TryCatch
                }
                catch (Exception ex)
                {
                    LOGGER.Error($"屏幕“{options}”无法加载", ex);
                }
#endif
            }
        }

        public void OnTick()
        {
            foreach (var context in Items)
            {
                var state = context.Value.StateManager;
                if (state.CurrentState == ScreenState.NotLoaded)
                {
                    if (state.NextState == ScreenState.Active)
                    {
                        if (!_saves.ContainsScreenOption(context.Value.Screen))
                        {
                            _saves.Add(new(context.Value.Screen));
                            SaveScreens();
                        }
                    }
                }
                else if (state.CurrentState == ScreenState.Active)
                {
                    if (ScreenConfig.ScreenIdleTimeout != -1 && context.Value.ScreenInputHandler.IdleTime >= ScreenConfig.ScreenIdleTimeout)
                    {
                        context.Value.UnloadScreen();
                        LOGGER.Warn($"ID为{context.Value.ID}的屏幕已达到最大闲置时间，即将卸载");
                    }
                }

                context.Value.OnTick();

                if (state.CurrentState == ScreenState.Unload)
                {
                    Items.Remove(context.Key);
                    _saves.Remove(new(context.Value.Screen));
                    SaveScreens();
                    if (context.Value.IsRestarting)
                        Items.Add(new(context.Value.Screen)).LoadScreen();
                }
            }
        }

        private void ReadScreens()
        {
            McbsDataDirectory? directory = MCOS.Instance.MinecraftInstance.MinecraftDirectory.GetActiveWorldDirectory()?.GetMcbsDataDirectory();
            if (directory is null)
                return;

            if (!File.Exists(directory.ScreenDataFile))
                return;

            string json = File.ReadAllText(directory.ScreenDataFile);
            ScreenOptions.Model[] items = JsonConvert.DeserializeObject<ScreenOptions.Model[]>(json) ?? throw new FormatException();
            foreach (var item in items)
            {
                ScreenOptions options = new(item);
                if (!_saves.ContainsScreenOption(options))
                    _saves.Add(options);
            }
        }

        private void SaveScreens()
        {
            McbsDataDirectory? directory = MCOS.Instance.MinecraftInstance.MinecraftDirectory.GetActiveWorldDirectory()?.GetMcbsDataDirectory();
            if (directory is null)
                return;

            List<ScreenOptions.Model> items = new();
            foreach (var save in _saves)
                items.Add(save.ToModel());
            string json = JsonConvert.SerializeObject(items);
            File.WriteAllText(directory.ScreenDataFile, json);
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

        public class ScreenCollection : IDictionary<int, ScreenContext>
        {
            public ScreenCollection(ScreenManager owner)
            {
                ArgumentNullException.ThrowIfNull(owner, nameof(owner));

                _owner = owner;
                _items = new();
                _id = 0;
            }

            private readonly ScreenManager _owner;

            private readonly ConcurrentDictionary<int, ScreenContext> _items;

            private int _id;

            public ICollection<int> Keys => _items.Keys;

            public ICollection<ScreenContext> Values => _items.Values;

            public int Count => _items.Count;

            public bool IsReadOnly => false;

            public ScreenContext this[int id] => _items[id];

            ScreenContext IDictionary<int, ScreenContext>.this[int key] { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

            public ScreenContext Add(Screen screen)
            {
                ArgumentNullException.ThrowIfNull(screen, nameof(screen));

                lock (_items)
                {
                    foreach (var value in Values)
                    {
                        if (value.Screen.EqualsScreenOption(screen))
                            throw new ArgumentException("尝试加载相同的屏幕", nameof(screen));
                    }

                    int id = _id;
                    ProcessContext process = MCOS.Instance.RunServicesApp();
                    IScreenView form = ((IServicesProgram)process.Program).ScreenView;
                    ScreenContext context = new(screen, form);
                    context.ID = id;
                    _items.TryAdd(id, context);
                    _owner.AddedScreen.Invoke(_owner, new(context));
                    _id++;
                    return context;
                }
            }

            public bool Remove(int id)
            {
                lock (_items)
                {
                    if (!_items.TryGetValue(id, out var context) || !_items.TryRemove(id, out _))
                        return false;

                    context.ID = -1;
                    _owner.RemovedScreen.Invoke(_owner, new(context));
                    return true;
                }
            }

            public void Clear()
            {
                foreach (var id in _items.Keys.ToArray())
                    Remove(id);
            }

            public bool ContainsKey(int id)
            {
                return _items.ContainsKey(id);
            }

            public bool TryGetValue(int id, [MaybeNullWhen(false)] out ScreenContext context)
            {
                return _items.TryGetValue(id, out context);
            }

            public IEnumerator<KeyValuePair<int, ScreenContext>> GetEnumerator()
            {
                return _items.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable)_items).GetEnumerator();
            }

            void ICollection<KeyValuePair<int, ScreenContext>>.Add(KeyValuePair<int, ScreenContext> item)
            {
                throw new NotSupportedException();
            }

            bool ICollection<KeyValuePair<int, ScreenContext>>.Remove(KeyValuePair<int, ScreenContext> item)
            {
                throw new NotSupportedException();
            }

            bool ICollection<KeyValuePair<int, ScreenContext>>.Contains(KeyValuePair<int, ScreenContext> item)
            {
                throw new NotSupportedException();
            }

            void ICollection<KeyValuePair<int, ScreenContext>>.CopyTo(KeyValuePair<int, ScreenContext>[] array, int arrayIndex)
            {
                throw new NotSupportedException();
            }

            void IDictionary<int, ScreenContext>.Add(int key, ScreenContext value)
            {
                throw new NotSupportedException();
            }
        }
    }
}
