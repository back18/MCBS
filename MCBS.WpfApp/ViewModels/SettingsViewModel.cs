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
        public SettingsViewModel(PageNavigateCommand pageNavigateCommand, IServiceProvider serviceProvider)
        {
            ArgumentNullException.ThrowIfNull(pageNavigateCommand, nameof(pageNavigateCommand));
            ArgumentNullException.ThrowIfNull(serviceProvider, nameof(serviceProvider));

            PageNavigateCommand = pageNavigateCommand;
        }

        public PageNavigateCommand PageNavigateCommand { get; }
    }
}
