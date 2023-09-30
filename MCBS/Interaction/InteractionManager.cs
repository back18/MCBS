﻿using log4net.Core;
using MCBS.Directorys;
using MCBS.Events;
using MCBS.Logging;
using Newtonsoft.Json;
using QuanLib.Core;
using QuanLib.Minecraft.Command;
using QuanLib.Minecraft.Command.Senders;
using QuanLib.Minecraft.Selectors;
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
    public class InteractionManager
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
            InteractionsDirectory? directory = MCOS.Instance.MinecraftInstance.MinecraftDirectory.GetActiveWorldDirectory()?.GetMcbsSavesDirectory()?.InteractionsDir;
            if (directory is null)
                return;
            directory.CreateIfNotExists();
            string[] files = directory.GetFiles("*.json");
            LOGGER.Info($"开始回收交互实体，共计{files.Length}个");
            foreach (string file in files)
            {
                try
                {
                    InteractionContext.Json json = JsonConvert.DeserializeObject<InteractionContext.Json>(File.ReadAllText(file)) ?? throw new FormatException();
                    EntityPos position = new(json.Position[0], json.Position[1], json.Position[2]);
                    BlockPos blockPos = position.ToBlockPos();
                    CommandSender sender = MCOS.Instance.MinecraftInstance.CommandSender;
                    sender.AddForceloadChunk(blockPos);
                    sender.KillEntity(json.EntityUUID);
                    sender.RemoveForceloadChunk(blockPos);
                    File.Delete(file);
                    LOGGER.Info($"玩家[{json.PlayerUUID}]的交互实体已回收");
                }
                catch (Exception ex)
                {
                    LOGGER.Error("无法回收交互实体", ex);
                }
            }
        }

        public void InteractionScheduling()
        {
            foreach (var item in Items)
            {
                item.Value.Handle();
                if (item.Value.InteractionState == InteractionState.Closed)
                    Items.Remove(item.Key);
            }
        }

        public class InteractionCollection : IDictionary<string, InteractionContext>
        {
            public InteractionCollection(InteractionManager owner)
            {
                _owner = owner ?? throw new ArgumentNullException(nameof(owner));
                _items = new();
            }

            private readonly InteractionManager _owner;

            private readonly ConcurrentDictionary<string, InteractionContext> _items;

            public InteractionContext this[string player] => _items[player];

            InteractionContext IDictionary<string, InteractionContext>.this[string key] { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

            public ICollection<string> Keys => _items.Keys;

            public ICollection<InteractionContext> Values => _items.Values;

            public int Count => _items.Count;

            public bool IsReadOnly => false;

            public bool TryAdd(string player, [MaybeNullWhen(false)] out InteractionContext interaction)
            {
                if (player is null)
                    throw new ArgumentNullException(nameof(player));

                lock (this)
                {
                    if (_items.ContainsKey(player))
                        goto err;

                    if (!InteractionContext.TryCreate(player, out interaction))
                        goto err;

                    _items.TryAdd(interaction.PlayerName, interaction);
                    _owner.AddedInteraction.Invoke(_owner, new(interaction));
                    return true;

                    err:
                    interaction = null;
                    return false;
                }
            }

            public bool Remove(string player)
            {
                lock (this)
                {
                    if (!_items.TryGetValue(player, out var interaction) || !_items.TryRemove(player, out _))
                        return false;

                    _owner.RemovedInteraction.Invoke(_owner, new(interaction));
                    return true;
                }
            }

            public void Clear()
            {
                foreach (var player in _items.Keys)
                    Remove(player);
            }

            public bool ContainsKey(string player)
            {
                return _items.ContainsKey(player);
            }

            public bool TryGetValue(string player, [MaybeNullWhen(false)] out InteractionContext interaction)
            {
                return _items.TryGetValue(player, out interaction);
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