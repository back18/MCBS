using CommunityToolkit.Mvvm.Messaging;
using MCBS.WpfApp.Attributes;
using MCBS.WpfApp.Messages;
using MCBS.WpfApp.ViewModels.Home;
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

namespace MCBS.WpfApp.Pages.Home
{
    /// <summary>
    /// LaunchPage.xaml 的交互逻辑
    /// </summary>
    [Route(Parent = typeof(HomePage))]
    public partial class LaunchPage : Page
    {
        public LaunchPage(LaunchViewModel launchViewModel)
        {
            ArgumentNullException.ThrowIfNull(launchViewModel, nameof(launchViewModel));
            DataContext = launchViewModel;

            InitializeComponent();
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);

            if (e.Cancel)
                return;

            WeakReferenceMessenger.Default.Send(new PageNavigatingFromMessage(e), nameof(LaunchPage));
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            WeakReferenceMessenger.Default.Send(new PageNavigatedToMessage(e), nameof(LaunchPage));
        }
    }
}
