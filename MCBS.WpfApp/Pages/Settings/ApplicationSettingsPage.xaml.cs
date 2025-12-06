using MCBS.WpfApp.Config;
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
    /// ApplicationSettingsPage.xaml 的交互逻辑
    /// </summary>
    public partial class ApplicationSettingsPage : Page, INavigationPage
    {
        public ApplicationSettingsPage(Page parentPage, IConfigProvider configProvider)
        {
            ArgumentNullException.ThrowIfNull(parentPage, nameof(parentPage));
            ArgumentNullException.ThrowIfNull(configProvider, nameof(configProvider));

            _parentPage = parentPage;
            _configProvider = configProvider;

            InitializeComponent();
        }

        private readonly Page _parentPage;

        private readonly IConfigProvider _configProvider;

        public Page GetParentPage()
        {
            return _parentPage;
        }
    }
}
