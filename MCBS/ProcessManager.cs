﻿using MCBS.Event;
using MCBS.UI;
using QuanLib.Core;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MCBS
{
    public class ProcessManager
    {
        public ProcessManager()
        {
            Items = new(this);
            AddedProcess += OnAddedProcess;
            RemovedProcess += OnRemovedProcess;
        }

        public ProcessCollection Items { get; }

        public event EventHandler<ProcessManager, ProcessEventArgs> AddedProcess;

        public event EventHandler<ProcessManager, ProcessEventArgs> RemovedProcess;

        protected virtual void OnAddedProcess(ProcessManager sender, ProcessEventArgs e) { }

        protected virtual void OnRemovedProcess(ProcessManager sender, ProcessEventArgs e) { }

        public void ProcessScheduling()
        {
            foreach (var process in Items)
            {
                process.Value.Handle();
                if (process.Value.ProcessState == ProcessState.Stopped)
                    Items.Remove(process.Key);
            }
        }

        public class ProcessCollection : IDictionary<int, Process>
        {
            public ProcessCollection(ProcessManager owner)
            {
                _owner = owner ?? throw new ArgumentNullException(nameof(owner));
                _items = new();
                _id = 0;
            }

            private readonly ProcessManager _owner;

            private readonly ConcurrentDictionary<int, Process> _items;

            private int _id;

            public Process this[int id] => _items[id];

            Process IDictionary<int, Process>.this[int key] { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

            public ICollection<int> Keys => _items.Keys;

            public ICollection<Process> Values => _items.Values;

            public int Count => _items.Count;

            public bool IsReadOnly => false;

            public Process Add(ApplicationInfo appInfo, IForm? initiator = null)
            {
                return Add(appInfo, Array.Empty<string>(), initiator);
            }

            public Process Add(ApplicationInfo APPInfo, string[] args, IForm? initiator = null)
            {
                if (APPInfo is null)
                    throw new ArgumentNullException(nameof(APPInfo));
                if (args is null)
                    throw new ArgumentNullException(nameof(args));

                lock (this)
                {
                    int id = _id;
                    Process process = new(APPInfo, args, initiator);
                    process.ID = id;
                    _items.TryAdd(id, process);
                    _owner.AddedProcess.Invoke(_owner, new(process));
                    _id++;
                    return process;
                }
            }

            public bool Remove(int id)
            {
                lock (this)
                {
                    if (!_items.TryGetValue(id, out var process) || !_items.TryRemove(id, out _))
                        return false;

                    process.ID = -1;
                    _owner.RemovedProcess.Invoke(_owner, new(process));
                    return true;
                }
            }

            public void Clear()
            {
                foreach (var id in _items.Keys)
                    Remove(id);
            }

            public bool ContainsKey(int id)
            {
                return _items.ContainsKey(id);
            }

            public bool TryGetValue(int id, [MaybeNullWhen(false)] out Process process)
            {
                return _items.TryGetValue(id, out process);
            }

            public IEnumerator<KeyValuePair<int, Process>> GetEnumerator()
            {
                return _items.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable)_items).GetEnumerator();
            }

            void ICollection<KeyValuePair<int, Process>>.Add(KeyValuePair<int, Process> item)
            {
                throw new NotSupportedException();
            }

            bool ICollection<KeyValuePair<int, Process>>.Remove(KeyValuePair<int, Process> item)
            {
                throw new NotSupportedException();
            }

            bool ICollection<KeyValuePair<int, Process>>.Contains(KeyValuePair<int, Process> item)
            {
                throw new NotSupportedException();
            }

            void ICollection<KeyValuePair<int, Process>>.CopyTo(KeyValuePair<int, Process>[] array, int arrayIndex)
            {
                throw new NotSupportedException();
            }

            void IDictionary<int, Process>.Add(int key, Process value)
            {
                throw new NotSupportedException();
            }
        }
    }
}