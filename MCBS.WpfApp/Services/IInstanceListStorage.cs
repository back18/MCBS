using MCBS.WpfApp.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.WpfApp.Services
{
    public interface IInstanceListStorage
    {
        public event EventHandler? InstanceIndexChanged;

        public IConfigStorage GetInstanceStorage(string instanceName);

        public MinecraftInstanceIndex GetInstanceIndex();

        public Task<MinecraftInstanceIndex> GetInstanceIndexAsync();

        public void SetCurrentInstance(string instanceName);

        public Task SetCurrentInstanceAsync(string instanceName);

        public void AddInstance(MinecraftInstanceConfig.Model instance);

        public Task AddInstanceAsync(MinecraftInstanceConfig.Model instance);

        public bool RemoveInstance(string instanceName);

        public Task<bool> RemoveInstanceAsync(string instanceName);

        public bool RenameInstance(string oldName, string newName);

        public Task<bool> RenameInstanceAsync(string oldName, string newName);
    }
}
