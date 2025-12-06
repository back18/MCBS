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
    /// DebuggerPage.xaml 的交互逻辑
    /// </summary>
    public partial class DebuggerPage : Page
    {
        public DebuggerPage()
        {
            InitializeComponent();

            _pageFactory = new PageFactory();
        }

        private readonly IPageFactory _pageFactory;

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            DebuggerNavigationView.SelectedItem = DebuggerNavigationView.MenuItems[0];
        }

        private void DebuggerNavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {
                return;
            }
            else if (args.SelectedItemContainer.Tag is Type pageType && _pageFactory.TryGetPage(pageType, out var page))
            {
                DebuggerFrame.Navigate(page);
            }
        }

        private void DebuggerFrame_Navigated(object sender, NavigationEventArgs e)
        {
            DebuggerNavigationView.IsBackEnabled = DebuggerFrame.CanGoBack;
            Type pageType = e.Content.GetType();

            if (pageType.Equals(DebuggerNavigationView.SettingsItem?.GetType()))
            {
                return;
            }
            else if (NavigationViewItemOf(pageType) is NavigationViewItem navigationViewItem)
            {
                DebuggerNavigationView.SelectedItem = navigationViewItem;
            }
        }

        private NavigationViewItem? NavigationViewItemOf(Type pageType)
        {
            ArgumentNullException.ThrowIfNull(pageType, nameof(pageType));

            foreach (NavigationViewItem navigationViewItem in DebuggerNavigationView.MenuItems)
            {
                if (pageType.Equals(navigationViewItem.Tag))
                    return navigationViewItem;
            }

            return null;
        }
    }
}
