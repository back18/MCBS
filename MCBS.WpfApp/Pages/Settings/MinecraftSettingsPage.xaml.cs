using MCBS.WpfApp.Config;
using MCBS.WpfApp.Config.Extensions;
using MCBS.WpfApp.ViewModels.Settings;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Page = iNKORE.UI.WPF.Modern.Controls.Page;

namespace MCBS.WpfApp.Pages.Settings
{
    /// <summary>
    /// MinecraftSettingsPage.xaml 的交互逻辑
    /// </summary>
    [Route(Parent = typeof(SettingsPage))]
    public partial class MinecraftSettingsPage : Page
    {
        public MinecraftSettingsPage(IConfigProvider configProvider)
        {
            ArgumentNullException.ThrowIfNull(configProvider, nameof(configProvider));

            _configProvider = configProvider;

            InitializeComponent();
        }

        private readonly IConfigProvider _configProvider;

        private MinecraftSettingsViewModel? _viewModel;

        public INavigationPage? GetParentPage()
        {
            return null;
        }

        public Type GetParentPageType()
        {
            return _parentPageType;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (_viewModel is not null)
                return;

            IConfigStorage configStorage = _configProvider.GetMinecraftConfigService();
            IConfigService configService = await configStorage.LoadOrCreateConfigAsync(true);

            _viewModel = new(NavigationService, configService);
            DataContext = _viewModel;

            var config = configService.GetCurrentConfig();
            Content = await SettingsUIBuilder.BuildSettingsUIAsync(config);
        }
    }
}
