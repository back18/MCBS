using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.WpfApp.Config
{
    public interface IConfigStorage
    {
        public bool IsExists { get; }

        public bool IsLoaded { get; }

        public event EventHandler Saved;

        public IConfigModel GetModel();

        public IConfigService GetConfig();

        public IConfigService CreateConfig(bool save);

        public Task<IConfigService> CreateConfigAsync(bool save);

        public IConfigService LoadConfig();

        public Task<IConfigService> LoadConfigAsync();

        public void SaveConfig();

        public Task SaveConfigAsync();
    }
}
