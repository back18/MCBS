using CommunityToolkit.Mvvm.ComponentModel;
using MCBS.WpfApp.Commands;
using MCBS.WpfApp.Config;
using System;
using System.Collections.Generic;
using System.Text;
using Page = iNKORE.UI.WPF.Modern.Controls.Page;

namespace MCBS.WpfApp.ViewModels.Settings
{
    public class SettingsViewModel : ObservableObject
    {
        public SettingsViewModel(Page page)
        {
            ArgumentNullException.ThrowIfNull(page, nameof(page));

            _pageCreateFactory = new PageCreateFactory();
            _configProvider = new ConfigProvider();

            PageNavigateCommand = new(page.Frame, _pageCreateFactory, [page, _configProvider]);
        }

        private readonly IPageCreateFactory _pageCreateFactory;

        private readonly IConfigProvider _configProvider;

        public PageNavigateCommand PageNavigateCommand { get; }
    }
}
