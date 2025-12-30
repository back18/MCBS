using iNKORE.UI.WPF.Modern.Controls;
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

namespace MCBS.WpfApp.Pages
{
    /// <summary>
    /// ManagerPage.xaml 的交互逻辑
    /// </summary>
    public partial class ManagerPage : Page
    {
        public ManagerPage(IServiceProvider serviceProvider)
        {
            ArgumentNullException.ThrowIfNull(serviceProvider, nameof(serviceProvider));

            InitializeComponent();

            _serviceProvider = serviceProvider;
        }

        private readonly IServiceProvider _serviceProvider;

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ManagerNavigationView.SelectedItem = ManagerNavigationView.MenuItems[0];
        }

        private void ManagerNavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {
                return;
            }
            else if (args.SelectedItemContainer.Tag is Type pageType && _serviceProvider.GetService(pageType) is Page page)
            {
                ManagerFrame.Navigate(page);
            }
        }

        private void ManagerFrame_Navigated(object sender, NavigationEventArgs e)
        {
            ManagerNavigationView.IsBackEnabled = ManagerFrame.CanGoBack;
            Type pageType = e.Content.GetType();

            if (pageType.Equals(ManagerNavigationView.SettingsItem?.GetType()))
            {
                return;
            }
            else if (NavigationViewItemOf(pageType) is NavigationViewItem navigationViewItem)
            {
                ManagerNavigationView.SelectedItem = navigationViewItem;
            }
        }

        private NavigationViewItem? NavigationViewItemOf(Type pageType)
        {
            ArgumentNullException.ThrowIfNull(pageType, nameof(pageType));

            foreach (NavigationViewItem navigationViewItem in ManagerNavigationView.MenuItems)
            {
                if (pageType.Equals(navigationViewItem.Tag))
                    return navigationViewItem;
            }

            return null;
        }
    }
}
