using MCBS.ObjectModel;
using QuanLib.Core.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Processes
{
    public partial class ProcessManager
    {
        public class ProcessCollection : IEnumerable<ProcessContext>
        {
            public ProcessCollection(ProcessManager owner)
            {
                ArgumentNullException.ThrowIfNull(owner, nameof(owner));

                _owner = owner;
                _items = [];

                _items.Added += Items_Added;
                _items.Removed += Items_Removed;
            }

            private readonly ProcessManager _owner;

            private readonly GuidDictionary<ProcessContext> _items;

            public int Count => _items.Count;

            public ProcessContext GetProcess(Guid guid)
            {
                return _items[guid];
            }

            public ProcessContext GetProcess(string shortId)
            {
                return _items[shortId];
            }

            public ProcessContext[] GetProcesses()
            {
                return _items.Values.ToArray();
            }

            internal void AddProcess(ProcessContext processContext)
            {
                _items.Add(processContext);
            }

            internal bool RemoveProcess(Guid guid)
            {
                return _items.Remove(guid);
            }

            internal bool RemoveProcess(string shortId)
            {
                return _items.Remove(shortId);
            }

            internal bool RemoveProcess(ProcessContext processContext)
            {
                return _items.Remove(processContext);
            }

            internal void ClearAllProcess()
            {
                _items.Clear();
            }

            public bool ContainsProcess(Guid guid)
            {
                return _items.ContainsKey(guid);
            }

            public bool ContainsProcess(string shortId)
            {
                return _items.ContainsKey(shortId);
            }

            public bool ContainsProcess(ProcessContext ProcessContext)
            {
                ArgumentNullException.ThrowIfNull(ProcessContext, nameof(ProcessContext));

                return _items.ContainsKey(ProcessContext.Guid);
            }

            public bool TryGetProcess(Guid guid, [MaybeNullWhen(false)] out ProcessContext result)
            {
                return _items.TryGetValue(guid, out result);
            }

            public bool TryGetProcess(string shortId, [MaybeNullWhen(false)] out ProcessContext result)
            {
                return _items.TryGetValue(shortId, out result);
            }

            public Guid PreGenerateGuid()
            {
                return _items.PreGenerateGuid();
            }

            private void Items_Added(GuidDictionary<ProcessContext> sender, EventArgs<ProcessContext> e)
            {
                _owner.AddedProcess.Invoke(_owner, e);
            }

            private void Items_Removed(GuidDictionary<ProcessContext> sender, EventArgs<ProcessContext> e)
            {
                _owner.RemovedProcess.Invoke(_owner, e);
            }

            public IEnumerator<ProcessContext> GetEnumerator()
            {
                return _items.Values.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}
