using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Navigation;
using Page = iNKORE.UI.WPF.Modern.Controls.Page;

namespace MCBS.WpfApp.Commands
{
    public class PageNavigateCommand : IRelayCommand
    {
        public PageNavigateCommand(INavigable navigable, IServiceProvider serviceProvider, Func<object?, bool>? canExecute = null)
        {
            ArgumentNullException.ThrowIfNull(navigable, nameof(navigable));
            ArgumentNullException.ThrowIfNull(serviceProvider, nameof(serviceProvider));

            _navigable = navigable;
            _serviceProvider = serviceProvider;
            _canExecute = canExecute;
        }

        private readonly INavigable _navigable;

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

            _navigable.NavigationService.Navigate(page);
        }

        public void NotifyCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
