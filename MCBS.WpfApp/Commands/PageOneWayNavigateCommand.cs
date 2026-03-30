using CommunityToolkit.Mvvm.Input;
using MCBS.WpfApp.Helpers;
using MCBS.WpfApp.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;

namespace MCBS.WpfApp.Commands
{
    public class PageOneWayNavigateCommand : IRelayCommand
    {
        public PageOneWayNavigateCommand(INavigationProvider navigationProvider, IServiceProvider serviceProvider, Func<object?, bool>? canExecute = null)
        {
            ArgumentNullException.ThrowIfNull(navigationProvider, nameof(navigationProvider));
            ArgumentNullException.ThrowIfNull(serviceProvider, nameof(serviceProvider));

            _navigationProvider = navigationProvider;
            _serviceProvider = serviceProvider;
            _canExecute = canExecute;
        }

        private readonly INavigationProvider _navigationProvider;

        private readonly IServiceProvider _serviceProvider;

        private readonly Func<object?, bool>? _canExecute;

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            return _canExecute is null || _canExecute.Invoke(parameter);
        }

        public void Execute(object? parameter)
        {
            if (parameter is not Type pageType || _serviceProvider.GetService(pageType) is not Page page)
                return;

            _navigationProvider.NavigationService.NavigateOnly(page);
        }

        public void NotifyCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
