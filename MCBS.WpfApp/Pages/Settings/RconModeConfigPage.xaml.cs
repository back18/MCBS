using MCBS.WpfApp.Config;
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
    /// RconModeConfigPage.xaml 的交互逻辑
    /// </summary>
    public partial class RconModeConfigPage : Page, INavigationPage
    {
        public RconModeConfigPage(Page parentPage, IConfigService configService)
        {
            ArgumentNullException.ThrowIfNull(parentPage, nameof(parentPage));
            ArgumentNullException.ThrowIfNull(configService, nameof(configService));

            _parentPage = parentPage;
            _configService = configService;

            InitializeComponent();
        }

        private readonly Page _parentPage;

        private readonly IConfigService _configService;

        private RconModeConfigViewModel? _viewModel;

        public Page GetParentPage()
        {
            return _parentPage;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (_viewModel is not null)
                return;

            _viewModel = new(_configService);
            DataContext = _viewModel;

            var config = _configService.GetCurrentConfig();
            Content = await SettingsUIBuilder.BuildSettingsUIAsync(config);
        }
    }
}
