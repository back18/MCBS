using log4net.Core;
using MCBS.Directorys;
using MCBS.Events;
using MCBS.Logging;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using QuanLib.Core;
using QuanLib.Minecraft.Command;
using QuanLib.Minecraft.Command.Senders;
using QuanLib.Minecraft.Vector;
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
    public class InteractionManager : ITickable
    {
        private static readonly LogImpl LOGGER = LogUtil.GetLogger();

        public InteractionManager()
        {
            Items = new(this);
            AddedInteraction += OnAddedInteraction;
            RemovedInteraction += OnRemovedInteraction;
        }

        public InteractionCollection Items { get; }

        public event EventHandler<InteractionManager, InteractionEventArgs> AddedInteraction;

        public event EventHandler<InteractionManager, InteractionEventArgs> RemovedInteraction;

        protected virtual void OnAddedInteraction(InteractionManager sender, InteractionEventArgs e) { }

        protected virtual void OnRemovedInteraction(InteractionManager sender, InteractionEventArgs e) { }

        public void Initialize()
        {
            InteractionsDirectory? directory = MinecraftBlockScreen.Instance.MinecraftInstance.MinecraftDirectory.GetActiveWorldDirectory()?.GetMcbsDataDirectory()?.InteractionsDir;
            if (directory is null)
                return;
            directory.CreateIfNotExists();
            string[] files = directory.GetFiles("*.json");
            LOGGER.Info($"开始回收交互实体，共计{files.Length}个");
            foreach (string file in files)
            {
                try
                {
                    InteractionContext.Model model = JsonConvert.DeserializeObject<InteractionContext.Model>(File.ReadAllText(file)) ?? throw new FormatException();
                    EntityPos position = new(model.Position[0], model.Position[1], model.Position[2]);
                    BlockPos blockPos = position.ToBlockPos();
                    CommandSender sender = MinecraftBlockScreen.Instance.MinecraftInstance.CommandSender;
                    sender.AddForceloadChunk(blockPos);
                    sender.KillEntity(model.EntityUUID);
                    sender.RemoveForceloadChunk(blockPos);
                    File.Delete(file);
                    LOGGER.Info($"玩家({model.PlayerUUID})的交互实体({model.EntityUUID})已回收");
                }
                catch (Exception ex)
                {
                    LOGGER.Error("无法回收交互实体", ex);
                }
            }
        }

        public void OnTick()
        {
            foreach (var item in Items)
            {
                item.Value.OnTick();
                if (item.Value.InteractionState == InteractionState.Closed)
                    Items.Remove(item.Key);
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
