using MCBS.Config;
using MCBS.Config.Minecraft;
using MCBS.Services;
using Microsoft.Extensions.DependencyInjection;
using QuanLib.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.Common.Services
{
    public class TomlConfigLoader
    {
        public TomlConfigLoader(
            IConfigPathProvider pathProvider,
            [FromKeyedServices("TOML")] IConfigLoadService configLoadService,
            [FromKeyedServices("TOML")] IConfigSaveService configSaveService)
        {
            ArgumentNullException.ThrowIfNull(pathProvider, nameof(pathProvider));
            ArgumentNullException.ThrowIfNull(configLoadService, nameof(configLoadService));
            ArgumentNullException.ThrowIfNull(configSaveService, nameof(configSaveService));

            _pathProvider = pathProvider;
            _configLoadService = configLoadService;
            _configSaveService = configSaveService;
        }

        private readonly IConfigPathProvider _pathProvider;
        private readonly IConfigLoadService _configLoadService;
        private readonly IConfigSaveService _configSaveService;

        public void CreateIfNotExists()
        {
            _configSaveService.CreateIfNotExists<MinecraftConfig.Model>(_pathProvider.MinecraftConfig.FullName);
            _configSaveService.CreateIfNotExists<SystemConfig.Model>(_pathProvider.SystemConfig.FullName);
            _configSaveService.CreateIfNotExists<ScreenConfig.Model>(_pathProvider.ScreenConfig.FullName);
        }

        public ConfigManager.Model LoadAll()
        {
            MinecraftConfig minecraftConfig = Load<MinecraftConfig, MinecraftConfig.Model>(_pathProvider.MinecraftConfig);
            SystemConfig systemConfig = Load<SystemConfig, SystemConfig.Model>(_pathProvider.SystemConfig);
            ScreenConfig screenConfig = Load<ScreenConfig, ScreenConfig.Model>(_pathProvider.ScreenConfig);

            return new ConfigManager.Model()
            {
                MinecraftConfig = minecraftConfig,
                SystemConfig = systemConfig,
                ScreenConfig = screenConfig
            };
        }

        private TDataViewModel Load<TDataViewModel, TDataModel>(FileInfo fileInfo)
            where TDataViewModel : IDataViewModel<TDataViewModel>
            where TDataModel : IDataModel<TDataModel>
        {
            using FileStream fileStream = fileInfo.OpenRead();
            TDataModel dataModel = _configLoadService.Load<TDataModel>(fileStream);
            return TDataViewModel.FromDataModel(dataModel);
        }
    }
}
