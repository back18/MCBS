using CommunityToolkit.Mvvm.Messaging;
using iNKORE.UI.WPF.Modern.Controls;
using MCBS.WpfApp.Messages;
using MCBS.WpfApp.Pages;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel;
using System.Reflection;
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
    public partial class MainWindow : Window, INavigable
    {
        public MainWindow(IServiceProvider serviceProvider)
        {
            ArgumentNullException.ThrowIfNull(serviceProvider, nameof(serviceProvider));

            InitializeComponent();

            _serviceProvider = serviceProvider;
        }

        private readonly IServiceProvider _serviceProvider;

        private readonly Dictionary<Type, Type> _routeCache = [];

        public NavigationService NavigationService => MainFrame.NavigationService;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MainNavigationView.SelectedItem = MainNavigationView.MenuItems[0];
        }

        private void NavigationView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            Type? pageType = args.IsSettingsSelected ? typeof(SettingsPage) : args.SelectedItemContainer.Tag as Type;
            object? content = MainFrame.Content;

            if (content is not null && content.GetType() is Type type &&
               (type.Equals(pageType) || GetRootPageType(type).Equals(pageType)))
                return;

            if (args.IsSettingsSelected)
            {
                SettingsPage settingsPage = _serviceProvider.GetRequiredService<SettingsPage>();
                MainFrame.Navigate(settingsPage);
            }
            else if (pageType is not null && _serviceProvider.GetService(pageType) is Page page)
            {
                MainFrame.Navigate(page);
            }
        }

        private void MainFrame_Navigated(object sender, NavigationEventArgs e)
        {
            MainNavigationView.IsBackEnabled = MainFrame.CanGoBack;

            if (e.Content is not Page page)
                return;

            Type pageType = GetRootPageType(page.GetType());
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

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            WeakReferenceMessenger.Default.Send(new MainWindowClosingMessage(e));
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

        private Type GetRootPageType(Type pageType)
        {
            ArgumentNullException.ThrowIfNull(pageType, nameof(pageType));

            if (_routeCache.TryGetValue(pageType, out var cached))
            {
                if (cached.Equals(pageType))
                    return cached;
                else
                    return GetRootPageType(cached);
            }
            else if (pageType.GetCustomAttribute<RouteAttribute>() is RouteAttribute route && route.Parent is Type parent)
            {
                _routeCache[pageType] = parent;
                return GetRootPageType(parent);
            }
            else
            {
                _routeCache[pageType] = pageType;
                return pageType;
            }
        }
    }
}