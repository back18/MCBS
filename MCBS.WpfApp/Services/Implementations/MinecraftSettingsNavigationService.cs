using MCBS.Config;
using MCBS.WpfApp.Pages.Settings;
using Microsoft.Extensions.DependencyInjection;
using System;
using Page = iNKORE.UI.WPF.Modern.Controls.Page;

namespace MCBS.WpfApp.Services.Implementations
{
    public class MinecraftSettingsNavigationService : IMinecraftSettingsNavigationService
    {
        public MinecraftSettingsNavigationService(IServiceProvider serviceProvider, INavigationProvider navigationProvider)
        {
            ArgumentNullException.ThrowIfNull(serviceProvider, nameof(serviceProvider));
            ArgumentNullException.ThrowIfNull(navigationProvider, nameof(navigationProvider));

            _serviceProvider = serviceProvider;
            _navigationProvider = navigationProvider;
        }

        private readonly IServiceProvider _serviceProvider;

        private readonly INavigationProvider _navigationProvider;

        public bool NavigateToSubconfig(string identifier)
        {
            ArgumentException.ThrowIfNullOrEmpty(identifier, nameof(identifier));

            Page? page = identifier switch
            {
                nameof(MinecraftConfig.McapiModeConfig) => _serviceProvider.GetService<McapiModeConfigPage>(),
                nameof(MinecraftConfig.RconModeConfig) => _serviceProvider.GetService<RconModeConfigPage>(),
                nameof(MinecraftConfig.ConsoleModeConfig) => _serviceProvider.GetService<ConsoleModeConfigPage>(),
                _ => null,
            };

            return page is not null && _navigationProvider.NavigationService.Navigate(page);
        }
    }
}
