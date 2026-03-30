using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace MCBS.WpfApp.Helpers
{
    public static class NavigationHelper
    {
        public static NavigationService? GetNavigationService(this NavigationEventArgs e)
        {
            ArgumentNullException.ThrowIfNull(e, nameof(e));

            if (e.Navigator is Frame frame)
                return frame.NavigationService;

            if (e.Navigator is NavigationWindow navigationWindow)
                return navigationWindow.NavigationService;

            return null;
        }

        public static NavigationService? GetNavigationService(this NavigatingCancelEventArgs e)
        {
            ArgumentNullException.ThrowIfNull(e, nameof(e));

            if (e.Navigator is Frame frame)
                return frame.NavigationService;

            if (e.Navigator is NavigationWindow navigationWindow)
                return navigationWindow.NavigationService;

            return null;
        }

        public static void ClearBackStack(this NavigationService navigationService)
        {
            ArgumentNullException.ThrowIfNull(navigationService, nameof(navigationService));

            while (navigationService.CanGoBack)
                navigationService.RemoveBackEntry();
        }

        public static void NavigateOnly(this NavigationService navigationService, object content)
        {
            ArgumentNullException.ThrowIfNull(navigationService, nameof(navigationService));
            ArgumentNullException.ThrowIfNull(content, nameof(content));

            navigationService.Navigated += RemoveBackHandler;
            navigationService.Navigate(content);
        }

        private static void RemoveBackHandler(object sender, NavigationEventArgs e)
        {
            NavigationService? navigationService = GetNavigationService(e);
            if (navigationService is not null)
            {
                if (navigationService.CanGoBack)
                    navigationService.RemoveBackEntry();
                navigationService.Navigated -= RemoveBackHandler;
            }
        }
    }
}
