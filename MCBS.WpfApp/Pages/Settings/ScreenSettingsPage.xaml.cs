using CommunityToolkit.Mvvm.Messaging;
using MCBS.Config;
using MCBS.WpfApp.Config;
using MCBS.WpfApp.Messages;
using MCBS.WpfApp.ViewModels.Settings;
using QuanLib.Core.Events;
using System;
using System.Collections.Generic;
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

namespace MCBS.WpfApp.Pages.Settings
{
    /// <summary>
    /// ScreenSettingsPage.xaml 的交互逻辑
    /// </summary>
    [Attributes.Route(Parent = typeof(SettingsPage))]
    public partial class ScreenSettingsPage : Page
    {
        public ScreenSettingsPage(ScreenSettingsViewModel viewModel)
        {
            ArgumentNullException.ThrowIfNull(viewModel, nameof(viewModel));

            viewModel.Loaded += ViewModel_Loaded;
            DataContext = viewModel;
        
            InitializeComponent();
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);

            WeakReferenceMessenger.Default.Send(new PageNavigatingFromMessage(e), nameof(ScreenConfig));
        }

        private async void ViewModel_Loaded(object? sender, EventArgs<object> e)
        {
            Content = await SettingsUIBuilder.BuildSettingsUIAsync(e.Argument);
        }
    }
}
