using MCBS.WpfApp.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Windows.Navigation;

namespace MCBS.WpfApp
{
    public partial class MainWindow : IBackNavigationService
    {
        private readonly List<INavigationProvider> _navigationStack = [];

        public event EventHandler? NavigationChanged;

        public INavigationProvider RootNavigationProvider => this;

        public int StackDepth => _navigationStack.Count;

        public bool CanGoBack { get; private set; }

        public bool RequestGoBack()
        {
            NavigationService? navigationService = GetActiveNavigationService();
            if (navigationService is null || !navigationService.CanGoBack)
                return false;

            navigationService.GoBack();
            if (UpdateCanGoBack())
                NavigationChanged?.Invoke(this, EventArgs.Empty);

            return true;
        }

        public void NotifyNavigationChanged()
        {
            UpdateCanGoBack();
            NavigationChanged?.Invoke(this, EventArgs.Empty);
        }

        public void PushNavigation(INavigationProvider navigationProvider)
        {
            ArgumentNullException.ThrowIfNull(navigationProvider, nameof(navigationProvider));
            if (_navigationStack.Contains(navigationProvider))
                throw new InvalidOperationException("The navigation provider is already in the stack.");

            _navigationStack.Remove(navigationProvider);
            _navigationStack.Add(navigationProvider);
            UpdateCanGoBack();
            NavigationChanged?.Invoke(this, EventArgs.Empty);
        }

        public INavigationProvider PopNavigation()
        {
            if (_navigationStack.Count <= 1)
                throw new InvalidOperationException("Cannot pop the root navigation provider.");

            int index = _navigationStack.Count - 1;
            INavigationProvider popped = _navigationStack[index];
            _navigationStack.RemoveAt(index);
            UpdateCanGoBack();
            NavigationChanged?.Invoke(this, EventArgs.Empty);
            return popped;
        }

        private bool UpdateCanGoBack()
        {
            NavigationService? navigationService = GetActiveNavigationService();
            bool canGoBack = navigationService?.CanGoBack ?? false;

            if (CanGoBack != canGoBack)
            {
                CanGoBack = canGoBack;
                return true;
            }

            return false;
        }

        private NavigationService? GetActiveNavigationService()
        {
            for (int i = _navigationStack.Count - 1; i >= 0; i--)
            {
                INavigationProvider navigationProvider = _navigationStack[i];
                if (TryGetNavigationService(navigationProvider, out NavigationService? navigationService) && navigationService.CanGoBack)
                    return navigationService;
            }

            return null;
        }

        private static bool TryGetNavigationService(INavigationProvider? navigationProvider, [MaybeNullWhen(false)] out NavigationService navigationService)
        {
            try
            {
                navigationService = navigationProvider?.NavigationService;
                return navigationService is not null;
            }
            catch (InvalidOperationException)
            {
                navigationService = null;
                return false;
            }
        }
    }
}
