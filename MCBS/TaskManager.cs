using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS
{
    public class TaskManager
    {
        public TaskManager()
        {
            _stopwatch = new();
            _normalTasks = new();
            _tempTasks = new();
        }

        private readonly Stopwatch _stopwatch;

        private Task? _mainTask;

        private readonly ConcurrentQueue<Action> _normalTasks;

        private readonly ConcurrentQueue<Action> _tempTasks;

        public bool IsCompletedMainTask => _mainTask?.IsCompleted ?? true;

        public TimeSpan MainTaskRuntime => _stopwatch.Elapsed;

        public void Initialize()
        {
            //TODO
        }

        public void HandleAllTask()
        {
            //TODO
        }

        public void WaitForMainTask()
        {
            _mainTask?.Wait();
        }

        public async Task WaitForMainTaskAsync()
        {
            if (_mainTask is null)
                return;

            await _mainTask;
        }

        internal void SetMainTask(Task task)
        {
            ArgumentNullException.ThrowIfNull(task, nameof(task));

            _stopwatch.Restart();
            _mainTask = task.ContinueWith((t) => _stopwatch.Stop());
        }

        internal void SetMainTask(Func<Task> task)
        {
            ArgumentNullException.ThrowIfNull(task, nameof(task));

            _stopwatch.Restart();
            _mainTask = task.Invoke().ContinueWith((t) => _stopwatch.Stop());
        }

        public void AddNormalTask(Action task)
        {
            throw new NotSupportedException();

            //ArgumentNullException.ThrowIfNull(task, nameof(task));

            //_normalTasks.Enqueue(task);
        }

        public void AddTempTask(Action task)
        {
            throw new NotSupportedException();

            //ArgumentNullException.ThrowIfNull(task, nameof(task));

            //_tempTasks.Enqueue(task);
        }

        public void Clear()
        {
            _mainTask = null;
            _normalTasks.Clear();
            _tempTasks.Clear();
        }
    }
}
