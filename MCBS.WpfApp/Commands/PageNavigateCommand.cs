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
        public PageNavigateCommand(NavigationService navigationService, IPageCreateFactory pageCreateFactory, object?[]? args = null, Func<object?, bool>? canExecute = null)
        {
            ArgumentNullException.ThrowIfNull(navigationService, nameof(navigationService));
            ArgumentNullException.ThrowIfNull(pageCreateFactory, nameof(pageCreateFactory));

            _navigationService = navigationService;
            _pageCreateFactory = pageCreateFactory;
            _args = args;
            _canExecute = canExecute;
        }

        private readonly NavigationService _navigationService;

        private readonly IPageCreateFactory _pageCreateFactory;

        private readonly object?[]? _args;

        private readonly Func<object?, bool>? _canExecute;

        public event EventHandler? CanExecuteChanged;

        public bool CanExecute(object? parameter)
        {
            return _canExecute is null || _canExecute.Invoke(parameter);
        }

        public void Execute(object? parameter)
        {
            if (parameter is not Type pageType)
                return;

            Page page = _args is not null ?
                _pageCreateFactory.GetOrCreatePage(pageType, _args) :
                _pageCreateFactory.GetOrCreatePage(pageType);

            _navigationService.Navigate(page);
        }

        public void NotifyCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
