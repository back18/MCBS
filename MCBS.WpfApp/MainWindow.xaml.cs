using iNKORE.UI.WPF.Modern.Controls;
using MCBS.WpfApp.Pages;
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

namespace MCBS.WpfApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            _pageFactory = new PageFactory();
        }

        private readonly IPageFactory _pageFactory;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MainNavigationView.SelectedItem = MainNavigationView.MenuItems[0];
        }

        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            Type? pageType = args.IsSettingsSelected ? typeof(SettingsPage) : args.SelectedItemContainer.Tag as Type;
            object? content = MainFrame.Content;

            if (content is not null && content.GetType().Equals(pageType))
                return;
            else if (content is INavigationPage navigationPage && GetRootPage(navigationPage).GetType().Equals(pageType))
                return;

            if (args.IsSettingsSelected)
            {
                SettingsPage settingsPage = _pageFactory.GetPage<SettingsPage>();
                MainFrame.Navigate(settingsPage);
            }
            else if (pageType is not null && _pageFactory.TryGetPage(pageType, out var page))
            {
                MainFrame.Navigate(page);
            }
        }

        private void MainFrame_Navigated(object sender, NavigationEventArgs e)
        {
            MainNavigationView.IsBackEnabled = MainFrame.CanGoBack;

            if (e.Content is not Page page)
                return;

            if (page is INavigationPage navigationPage)
                page = GetRootPage(navigationPage);

            Type pageType = page.GetType();

            if (pageType.Equals(typeof(SettingsPage)))
            {
                MainNavigationView.SelectedItem = MainNavigationView.SettingsItem;
            }
            else if (NavigationViewItemOf(pageType) is NavigationViewItem navigationViewItem)
            {
                MainNavigationView.SelectedItem = navigationViewItem;
            }
        }

        private void MainNavigationView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            if (MainFrame.CanGoBack)
                MainFrame.GoBack();
        }

        private NavigationViewItem? NavigationViewItemOf(Type pageType)
        {
            ArgumentNullException.ThrowIfNull(pageType, nameof(pageType));

            foreach (NavigationViewItem navigationViewItem in MainNavigationView.MenuItems)
            {
                if (pageType.Equals(navigationViewItem.Tag))
                    return navigationViewItem;
            }

            return null;
        }

        private static Page GetRootPage(INavigationPage navigationPage)
        {
            ArgumentNullException.ThrowIfNull(navigationPage, nameof(navigationPage));

            Page page = navigationPage.GetParentPage();
            if (page is INavigationPage navigationPage2)
                return GetRootPage(navigationPage2);
            else
                return page;
        }
    }
}