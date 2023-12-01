using QuanLib.Minecraft.Command.Senders;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS
{
    public class TaskManager
    {
        public TaskManager()
        {
            _tasks = new();
            _tempTasks = new();
        }

        private readonly ConcurrentQueue<Action> _tasks;

        private readonly ConcurrentQueue<Action> _tempTasks;

        private Task? _previous;

        private Task? _current;

        public bool IsHandled { get; private set; }

        public bool IsCompletedPreviousMainTask => _previous?.IsCompleted ?? true;

        public bool IsCompletedCurrentMainTask => _current?.IsCompleted ?? true;

        public void Initialize()
        {
            //var twowayCommandSender = MCOS.Instance.MinecraftInstance.CommandSender.TwowaySender;
            //var onewayCommandSender = MCOS.Instance.MinecraftInstance.CommandSender.OnewaySender;
            //if (twowayCommandSender == onewayCommandSender)
            //{
            //    onewayCommandSender.WaitForResponseCallback += CommandSender_WaitForResponseCallback;
            //}
            //else
            //{
            //    twowayCommandSender.WaitForResponseCallback += CommandSender_WaitForResponseCallback;
            //    onewayCommandSender.WaitForResponseCallback += CommandSender_WaitForResponseCallback;
            //}
        }

        private void CommandSender_WaitForResponseCallback(ICommandSender sender, EventArgs e)
        {
            HandleAllTask();
        }

        public void HandleAllTask()
        {
            lock (this)
            {
                if (IsHandled)
                    return;
                IsHandled = true;

                Stopwatch stopwatch = Stopwatch.StartNew();
                _previous?.Wait();
                stopwatch.Stop();

                while (_tasks.TryDequeue(out var task))
                    task.Invoke();

                if (stopwatch.ElapsedMilliseconds > 50)
                {
                    _tempTasks.Clear();
                }
                else
                {
                    while (_tempTasks.TryDequeue(out var task))
                        task.Invoke();
                }
            }
        }

        public void WaitForPreviousMainTask()
        {
            _previous?.Wait();
        }

        public void WaitForCurrentMainTask()
        {
            _current?.Wait();
        }

        internal void ResetCurrentMainTask()
        {
            _previous = _current;
            IsHandled = false;
        }

        internal void SetCurrentMainTask(Task mainTask)
        {
            ArgumentNullException.ThrowIfNull(mainTask, nameof(mainTask));

            if (_current != mainTask)
                _current = mainTask;
            else
                _current = null;
        }

        public void AddTask(Action task)
        {
            ArgumentNullException.ThrowIfNull(task, nameof(task));

            _tasks.Enqueue(task);
        }

        public void AddTempTask(Action task)
        {
            ArgumentNullException.ThrowIfNull(task, nameof(task));

            _tempTasks.Enqueue(task);
        }

        public void Clear()
        {
            _previous = null;
            _current = null;
            _tasks.Clear();
            _tempTasks.Clear();
        }
    }
}
