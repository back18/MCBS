using CommunityToolkit.Mvvm.Messaging;
using MCBS.WpfApp.Attributes;
using MCBS.WpfApp.Helpers;
using MCBS.WpfApp.Messages;
using MCBS.WpfApp.Services;
using MCBS.WpfApp.Services.Implementations;
using MCBS.WpfApp.ViewModels.Home;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// InstanceManagePage.xaml 的交互逻辑
    /// </summary>
    [Route(Parent = typeof(LaunchPage))]
    public partial class InstanceManagePage : Page, INavigationProvider, IRecipient<MinecraftInstanceChangedMessage>
    {
        public InstanceManagePage(
            InstanceManageViewModel instanceManageViewModel,
            IInstancePageFactory instancePageFactory,
            IBackNavigationService backNavigationService)
        {
            ArgumentNullException.ThrowIfNull(instanceManageViewModel, nameof(instanceManageViewModel));
            ArgumentNullException.ThrowIfNull(instancePageFactory, nameof(instancePageFactory));
            ArgumentNullException.ThrowIfNull(backNavigationService, nameof(backNavigationService));

            DataContext = instanceManageViewModel;
            _instancePageFactory = instancePageFactory;
            _backNavigationService = backNavigationService;

            InitializeComponent();

            _frameNavigationProvider = new FrameNavigationProvider(CurrentInstanceSettingsFrame);
            WeakReferenceMessenger.Default.Register<MinecraftInstanceChangedMessage>(this);
        }

        private readonly FrameNavigationProvider _frameNavigationProvider;
        private readonly IInstancePageFactory _instancePageFactory;
        private readonly IBackNavigationService _backNavigationService;
        private readonly Dictionary<string, Page> _pageCache = [];
        private string _currentInstanceName = string.Empty;
        private string _nextInstanceName = string.Empty;

        NavigationService INavigationProvider.NavigationService => _frameNavigationProvider.NavigationService;

        void IRecipient<MinecraftInstanceChangedMessage>.Receive(MinecraftInstanceChangedMessage message)
        {
            _nextInstanceName = message.NewInstanceName;
            SwitchInstancePage(message.NewInstanceName);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            WeakReferenceMessenger.Default.Send(new PageNavigatedToMessage(e), nameof(InstanceManagePage));
            _backNavigationService.PushNavigation(this);
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);

            if (e.Cancel)
                return;

            WeakReferenceMessenger.Default.Send(new PageNavigatingFromMessage(e), nameof(InstanceManagePage));
            _backNavigationService.PopNavigation();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (_currentInstanceName != _nextInstanceName || CurrentInstanceSettingsFrame.Content is null)
                SwitchInstancePage(_nextInstanceName);
        }

        private void SwitchInstancePage(string? instanceName)
        {
            if (!IsLoaded && !string.IsNullOrEmpty(_currentInstanceName))
                return;

            Page? instancePage;
            if (string.IsNullOrEmpty(instanceName))
            {
                instancePage = _instancePageFactory.CreateEmpty();
                instancePage.DataContext = DataContext;
            }
            else if (!_pageCache.TryGetValue(instanceName, out instancePage))
            {
                instancePage = _instancePageFactory.Create(instanceName, _frameNavigationProvider);
            }

            _currentInstanceName = instanceName ?? string.Empty;
            _pageCache[_currentInstanceName] = instancePage;
            _frameNavigationProvider.NavigationService.NavigateOnly(instancePage);
            _backNavigationService.NotifyNavigationChanged();
        }
    }
}
