using MCBS.Config;
using MCBS.WpfApp.Config.Extensions;
using MCBS.WpfApp.Pages.Settings;
using MCBS.WpfApp.ViewModels.Settings;
using Microsoft.Extensions.Logging;
using System;
using Page = iNKORE.UI.WPF.Modern.Controls.Page;

namespace MCBS.WpfApp.Services.Implementations
{
    public sealed class ScopedMinecraftSettingsNavigationService : IMinecraftSettingsNavigationService
    {
        public ScopedMinecraftSettingsNavigationService(
            ILoggerFactory loggerFactory,
            IMessageBoxService messageBoxService,
            INavigationProvider navigationProvider,
            IConfigStorage configStorage)
        {
            ArgumentNullException.ThrowIfNull(loggerFactory, nameof(loggerFactory));
            ArgumentNullException.ThrowIfNull(messageBoxService, nameof(messageBoxService));
            ArgumentNullException.ThrowIfNull(navigationProvider, nameof(navigationProvider));
            ArgumentNullException.ThrowIfNull(configStorage, nameof(configStorage));

            _loggerFactory = loggerFactory;
            _messageBoxService = messageBoxService;
            _navigationProvider = navigationProvider;
            _configStorage = configStorage;
        }

        private readonly ILoggerFactory _loggerFactory;
        private readonly IMessageBoxService _messageBoxService;
        private readonly INavigationProvider _navigationProvider;
        private readonly IConfigStorage _configStorage;

        public bool NavigateToSubconfig(string identifier)
        {
            ArgumentException.ThrowIfNullOrEmpty(identifier, nameof(identifier));

            IConfigService configService = _configStorage.IsLoaded
                ? _configStorage.GetConfig()
                : _configStorage.LoadOrCreateConfig(false);
            var model = (MinecraftConfig.Model)configService.GetCurrentConfig();

            Page? page = identifier switch
            {
                nameof(MinecraftConfig.McapiModeConfig) => new McapiModeConfigPage(new McapiModeConfigViewModel(_loggerFactory, _messageBoxService, configService.CreateSubservices(model.McapiModeConfig))),
                nameof(MinecraftConfig.RconModeConfig) => new RconModeConfigPage(new RconModeConfigViewModel(_loggerFactory, _messageBoxService, configService.CreateSubservices(model.RconModeConfig))),
                nameof(MinecraftConfig.ConsoleModeConfig) => new ConsoleModeConfigPage(new ConsoleModeConfigViewModel(_loggerFactory, _messageBoxService, configService.CreateSubservices(model.ConsoleModeConfig))),
                _ => null,
            };

            return page is not null && _navigationProvider.NavigationService.Navigate(page);
        }
    }
}
