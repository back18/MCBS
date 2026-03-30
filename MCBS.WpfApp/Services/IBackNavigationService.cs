using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.WpfApp.Services
{
    public interface IBackNavigationService
    {
        public event EventHandler? NavigationChanged;

        public INavigationProvider RootNavigationProvider { get; }

        public int StackDepth { get; }

        public bool CanGoBack { get; }

        public bool RequestGoBack();

        public void NotifyNavigationChanged();

        public void PushNavigation(INavigationProvider navigationProvider);

        public INavigationProvider PopNavigation();
    }
}
