using Newtonsoft.Json;
using QuanLib.Core;
using QuanLib.Core.Events;
using QuanLib.Game;
using QuanLib.IO.Extensions;
using QuanLib.Logging;
using QuanLib.Minecraft.Command;
using QuanLib.Minecraft.Command.Senders;
using QuanLib.TickLoop;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Interaction
{
    public class InteractionManager : UnmanagedBase, ITickUpdatable
    {
        private static readonly ILogger LOGGER = Log4NetManager.Instance.GetLogger();

        public InteractionManager()
        {
            Items = new(this);
            AddedInteraction += OnAddedInteraction;
            RemovedInteraction += OnRemovedInteraction;
        }

        public InteractionCollection Items { get; }

        public event EventHandler<InteractionManager, EventArgs<InteractionContext>> AddedInteraction;

        public event EventHandler<InteractionManager, EventArgs<InteractionContext>> RemovedInteraction;

        protected virtual void OnAddedInteraction(InteractionManager sender, EventArgs<InteractionContext> e) { }

        protected virtual void OnRemovedInteraction(InteractionManager sender, EventArgs<InteractionContext> e) { }

        public void Initialize()
        {
            DirectoryInfo? worldDirectory = MinecraftBlockScreen.Instance.MinecraftInstance.MinecraftPathManager.GetActiveWorlds().FirstOrDefault();

            if (worldDirectory is null)
                return;

            McbsDataPathManager mcbsDataPathManager = McbsDataPathManager.FromWorldDirectoryCreate(worldDirectory.FullName);
            mcbsDataPathManager.McbsData_Interactions.CreateIfNotExists();
            string[] files = mcbsDataPathManager.McbsData_Interactions.GetFilePaths("*.json");

            foreach (string file in files)
            {
                try
                {
                    InteractionContext.Model model = JsonConvert.DeserializeObject<InteractionContext.Model>(File.ReadAllText(file)) ?? throw new FormatException();
                    Vector3<double> entityPos = new(model.Position[0], model.Position[1], model.Position[2]);
                    Vector3<int> blockPos = entityPos.ToIntVector3();
                    CommandSender sender = MinecraftBlockScreen.Instance.MinecraftInstance.CommandSender;
                    sender.AddForceloadChunk(blockPos);
                    sender.KillEntity(model.EntityUUID);
                    sender.RemoveForceloadChunk(blockPos);
                    File.Delete(file);

                    LOGGER.Info($"玩家({model.PlayerUUID})的交互实体({model.EntityUUID})已回收");
                }
                catch (Exception ex)
                {
                    LOGGER.Error($"无法回收位于“{Path.GetFileName(file)}”的交互实体", ex);
                }
            }
        }

        public void OnTickUpdate(int tick)
        {
            foreach (var item in Items)
            {
                item.Value.OnTickUpdate(tick);
                if (item.Value.InteractionState == InteractionState.Closed)
                    Items.Remove(item.Key);
            }
        }

        protected override void DisposeUnmanaged()
        {
            string[] players = Items.Keys.ToArray();
            for (int i = 0; i < players.Length; i++)
            {
                string player = players[i];
                if (Items.TryGetValue(player, out var screenContext))
                {
                    screenContext.Dispose();
                    Items.Remove(player);
                }
            }
        }

        public class InteractionCollection : IDictionary<string, InteractionContext>
        {
            public InteractionCollection(InteractionManager owner)
            {
                ArgumentNullException.ThrowIfNull(owner, nameof(owner));

                _owner = owner;
                _items = new();
            }

            private readonly InteractionManager _owner;

            private readonly ConcurrentDictionary<string, InteractionContext> _items;

            public InteractionContext this[string playerName] => _items[playerName];

            InteractionContext IDictionary<string, InteractionContext>.this[string key] { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

            public ICollection<string> Keys => _items.Keys;

            public ICollection<InteractionContext> Values => _items.Values;

            public int Count => _items.Count;

            public bool IsReadOnly => false;

            public InteractionContext Add(string playerName)
            {
                lock (_items)
                {
                    InteractionContext interactionContext = new(playerName);
                    _items.TryAdd(playerName, interactionContext);
                    _owner.AddedInteraction.Invoke(_owner, new(interactionContext));
                    return interactionContext;
                }
            }

            public bool Remove(string playerName)
            {
                lock (_items)
                {
                    if (!_items.TryRemove(playerName, out var interactionContext))
                        return false;

                    _owner.RemovedInteraction.Invoke(_owner, new(interactionContext));
                    return true;
                }
            }

            public void Clear()
            {
                foreach (var playerName in _items.Keys)
                    Remove(playerName);
            }

            public bool ContainsKey(string playerName)
            {
                return _items.ContainsKey(playerName);
            }

            public bool TryGetValue(string playerName, [MaybeNullWhen(false)] out InteractionContext interaction)
            {
                return _items.TryGetValue(playerName, out interaction);
            }

            public IEnumerator<KeyValuePair<string, InteractionContext>> GetEnumerator()
            {
                return _items.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable)_items).GetEnumerator();
            }

            void ICollection<KeyValuePair<string, InteractionContext>>.Add(KeyValuePair<string, InteractionContext> item)
            {
                ((ICollection<KeyValuePair<string, InteractionContext>>)_items).Add(item);
            }

            bool ICollection<KeyValuePair<string, InteractionContext>>.Remove(KeyValuePair<string, InteractionContext> item)
            {
                return ((ICollection<KeyValuePair<string, InteractionContext>>)_items).Remove(item);
            }

            bool ICollection<KeyValuePair<string, InteractionContext>>.Contains(KeyValuePair<string, InteractionContext> item)
            {
                return ((ICollection<KeyValuePair<string, InteractionContext>>)_items).Contains(item);
            }

            void ICollection<KeyValuePair<string, InteractionContext>>.CopyTo(KeyValuePair<string, InteractionContext>[] array, int arrayIndex)
            {
                ((ICollection<KeyValuePair<string, InteractionContext>>)_items).CopyTo(array, arrayIndex);
            }

            void IDictionary<string, InteractionContext>.Add(string key, InteractionContext value)
            {
                throw new NotSupportedException();
            }
        }
    }
}
