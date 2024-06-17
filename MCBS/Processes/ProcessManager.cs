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
    public partial class ProcessManager : ITickUpdatable
    {
        public ProcessManager()
        {
            Items = new(this);

            AddedProcess += OnAddedProcess;
            RemovedProcess += OnRemovedProcess;
        }

        public ProcessCollection Items { get; }

        public event EventHandler<ProcessManager, EventArgs<ProcessContext>> AddedProcess;

        public event EventHandler<ProcessManager, EventArgs<ProcessContext>> RemovedProcess;

        protected virtual void OnAddedProcess(ProcessManager sender, EventArgs<ProcessContext> e) { }

        protected virtual void OnRemovedProcess(ProcessManager sender, EventArgs<ProcessContext> e) { }

        public void OnTickUpdate(int tick)
        {
            foreach (var items in Items)
            {
                items.Value.OnTickUpdate(tick);
                if (items.Value.ProcessState == ProcessState.Stopped)
                    Items.TryRemove(items.Key, out _);
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

            ProcessContext processContext = new(applicationManifest, args, initiator);
            if (!Items.TryAdd(processContext.GUID, processContext))
                throw new InvalidOperationException();

            processContext.StartProcess();
            return processContext;
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
            if (!typeof(IProgram).IsAssignableFrom(applicationManifest.MainClass))
                throw new InvalidOperationException("无效的 IServicesProgram");

            return StartProcess(applicationManifest, args);
        }

        public ProcessContext StartServicesProcess()
        {
            return StartServicesProcess(Array.Empty<string>());
        }
    }
}
