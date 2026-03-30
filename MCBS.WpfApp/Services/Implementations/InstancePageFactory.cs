using MCBS.WpfApp.Pages.Home;
using MCBS.WpfApp.Pages.Settings;
using MCBS.WpfApp.ViewModels.Home;
using Microsoft.Extensions.Logging;
using Page = iNKORE.UI.WPF.Modern.Controls.Page;

namespace MCBS.WpfApp.Services.Implementations
{
    public class InstancePageFactory : IInstancePageFactory
    {
        public InstancePageFactory(
            ILoggerFactory loggerFactory,
            IMessageBoxService messageBoxService,
            IInstanceListStorage instanceListStorage)
        {
            ArgumentNullException.ThrowIfNull(loggerFactory, nameof(loggerFactory));
            ArgumentNullException.ThrowIfNull(messageBoxService, nameof(messageBoxService));
            ArgumentNullException.ThrowIfNull(instanceListStorage, nameof(instanceListStorage));

            _loggerFactory = loggerFactory;
            _messageBoxService = messageBoxService;
            _instanceListStorage = instanceListStorage;
        }

        private readonly ILoggerFactory _loggerFactory;
        private readonly IMessageBoxService _messageBoxService;
        private readonly IInstanceListStorage _instanceListStorage;

        public Page CreateEmpty()
        {
            return new EmptyInstancePage();
        }

        public Page Create(string instanceName, INavigationProvider navigationProvider)
        {
            ArgumentException.ThrowIfNullOrEmpty(instanceName, nameof(instanceName));
            ArgumentNullException.ThrowIfNull(navigationProvider, nameof(navigationProvider));

            IConfigStorage configStorage = _instanceListStorage.GetInstanceStorage(instanceName);
            IMinecraftSettingsNavigationService navigationService = new ScopedMinecraftSettingsNavigationService(
                _loggerFactory,
                _messageBoxService,
                navigationProvider,
                configStorage);
            InstanceSettingsViewModel viewModel = new(_loggerFactory, _messageBoxService, navigationService, configStorage, _instanceListStorage);

            return new MinecraftSettingsPage(viewModel);
        }
    }
}
