using MCBS.Application;
using MCBS.Config;
using MCBS.UI;
using QuanLib.Core;
using QuanLib.Core.Events;
using QuanLib.TickLoop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Processes
{
    public partial class ProcessManager : UnmanagedBase, ITickUpdatable
    {
        public ProcessManager()
        {
            Collection = new(this);

            AddedProcess += OnAddedProcess;
            RemovedProcess += OnRemovedProcess;
        }

        public ProcessCollection Collection { get; }

        public event EventHandler<ProcessManager, EventArgs<ProcessContext>> AddedProcess;

        public event EventHandler<ProcessManager, EventArgs<ProcessContext>> RemovedProcess;

        protected virtual void OnAddedProcess(ProcessManager sender, EventArgs<ProcessContext> e) { }

        protected virtual void OnRemovedProcess(ProcessManager sender, EventArgs<ProcessContext> e) { }

        public void OnTickUpdate(int tick)
        {
            foreach (ProcessContext processContext in Collection.GetProcesses())
            {
                processContext.OnTickUpdate(tick);
                if (processContext.ProcessState == ProcessState.Stopped)
                {
                    lock (Collection)
                        Collection.RemoveProcess(processContext);
                }
            }
        }

        public ProcessContext StartProcess(ApplicationManifest applicationManifest, IForm? initiator = null)
        {
            return StartProcess(applicationManifest, Array.Empty<string>(), initiator);
        }

        public ProcessContext StartProcess(ApplicationManifest applicationManifest, string[] args, IForm? initiator = null)
        {
            ArgumentNullException.ThrowIfNull(applicationManifest, nameof(applicationManifest));
            ArgumentNullException.ThrowIfNull(args, nameof(args));

            lock (Collection)
            {
                Guid guid = Collection.PreGenerateGuid();
                ProcessContext processContext = new(applicationManifest, guid, args, initiator);
                Collection.AddProcess(processContext);
                processContext.StartProcess();
                return processContext;
            }
        }

        public ProcessContext StartProcess(string appId, IForm? initiator = null)
        {
            ApplicationManifest applicationManifest = MinecraftBlockScreen.Instance.AppComponents[appId];
            return StartProcess(applicationManifest, Array.Empty<string>(), initiator);
        }

        public ProcessContext StartProcess(string appId, string[] args, IForm? initiator = null)
        {
            ApplicationManifest applicationManifest = MinecraftBlockScreen.Instance.AppComponents[appId];
            return StartProcess(applicationManifest, args, initiator);
        }

        public ProcessContext StartServicesProcess(string[] args)
        {
            ApplicationManifest applicationManifest = MinecraftBlockScreen.Instance.AppComponents[ConfigManager.SystemConfig.ServicesAppID];
            if (!typeof(IServicesProgram).IsAssignableFrom(applicationManifest.MainClass))
                throw new InvalidOperationException("无效的 IServicesProgram");

            return StartProcess(applicationManifest, args);
        }

        public ProcessContext StartServicesProcess()
        {
            return StartServicesProcess(Array.Empty<string>());
        }

        protected override void DisposeUnmanaged()
        {
            List<Task> tasks = [];
            foreach (ProcessContext processContext in Collection.GetProcesses())
                tasks.Add(Task.Run(processContext.Stop));

            Task.WaitAll(tasks.ToArray());
            Collection.ClearAllProcess();
        }
    }
}
