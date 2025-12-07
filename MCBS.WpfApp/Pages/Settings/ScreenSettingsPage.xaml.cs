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
    /// ScreenSettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class ScreenSettingsPage : Page, INavigationPage
    {
        public ScreenSettingsPage(Type parentPageType, IConfigProvider configProvider)
        {
            ArgumentNullException.ThrowIfNull(parentPageType, nameof(parentPageType));
            ArgumentNullException.ThrowIfNull(configProvider, nameof(configProvider));

            _parentPageType = parentPageType;
            _configProvider = configProvider;

            InitializeComponent();
        }

        private readonly Type _parentPageType;

        private readonly IConfigProvider _configProvider;

        private ScreenSettingsViewModel? _viewModel;

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

            IConfigStorage configStorage = _configProvider.GetScreenConfigService();
            IConfigService configService = await configStorage.LoadOrCreateConfigAsync(true);

            _viewModel = new(configService);
            DataContext = _viewModel;

            var config = configService.GetCurrentConfig();
            Content = await SettingsUIBuilder.BuildSettingsUIAsync(config);
        }
    }
}
