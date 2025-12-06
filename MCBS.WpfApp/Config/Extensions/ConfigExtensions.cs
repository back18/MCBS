using QuanLib.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.WpfApp.Config.Extensions
{
    public static class ConfigExtensions
    {
        extension(IConfigStorage configStorage)
        {
            public IConfigService LoadOrCreateConfig(bool save)
            {
                if (configStorage.IsExists)
                    return configStorage.LoadConfig();
                else
                    return configStorage.CreateConfig(save);
            }

            public Task<IConfigService> LoadOrCreateConfigAsync(bool save)
            {
                if (configStorage.IsExists)
                    return configStorage.LoadConfigAsync();
                else
                    return configStorage.CreateConfigAsync(save);
            }
        }

        extension (IConfigService configService)
        {
            public IConfigService CreateSubservices<TModel>(TModel subconfig) where TModel : IDataModel<TModel>
            {
                return new ConfigService(subconfig, new ConfigDataModel<TModel>(), configService.GetConfigStorage());
            }
        }
    }
}
