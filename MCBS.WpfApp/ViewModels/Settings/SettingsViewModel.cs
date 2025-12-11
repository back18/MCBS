using CommunityToolkit.Mvvm.ComponentModel;
using MCBS.WpfApp.Commands;
using MCBS.WpfApp.Config;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Navigation;

namespace MCBS.WpfApp.ViewModels.Settings
{
    public class SettingsViewModel : ObservableObject
    {
        public SettingsViewModel(NavigationService navigationService)
        {
            ArgumentNullException.ThrowIfNull(navigationService, nameof(navigationService));

            _pageCreateFactory = new PageCreateFactory();
            _configProvider = new ConfigProvider();

            PageNavigateCommand = new(navigationService, _pageCreateFactory, [_configProvider]);
        }

        private readonly IPageCreateFactory _pageCreateFactory;

        private readonly IConfigProvider _configProvider;

        public PageNavigateCommand PageNavigateCommand { get; }
    }
}
