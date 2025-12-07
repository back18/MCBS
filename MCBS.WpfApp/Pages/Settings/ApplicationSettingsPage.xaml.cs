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
        public ApplicationSettingsPage(Type parentPageType, IConfigProvider configProvider)
        {
            ArgumentNullException.ThrowIfNull(parentPageType, nameof(parentPageType));
            ArgumentNullException.ThrowIfNull(configProvider, nameof(configProvider));

            _parentPageType = parentPageType;
            _configProvider = configProvider;

            InitializeComponent();
        }

        private readonly Type _parentPageType;

        private readonly IConfigProvider _configProvider;

        public INavigationPage? GetParentPage()
        {
            return null;
        }

        public Type GetParentPageType()
        {
            return _parentPageType;
        }
    }
}
