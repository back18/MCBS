using CommunityToolkit.Mvvm.ComponentModel;
using MCBS.WpfApp.Commands;
using MCBS.WpfApp.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.WpfApp.ViewModels
{
    public class SettingsViewModel : ObservableObject
    {
        public SettingsViewModel(INavigable navigable, IServiceProvider serviceProvider)
        {
            ArgumentNullException.ThrowIfNull(navigable, nameof(navigable));
            ArgumentNullException.ThrowIfNull(serviceProvider, nameof(serviceProvider));

            PageNavigateCommand = new(navigable, serviceProvider);
        }

        public PageNavigateCommand PageNavigateCommand { get; }
    }
}
