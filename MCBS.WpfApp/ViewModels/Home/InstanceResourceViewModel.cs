using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MCBS.WpfApp.Commands;
using MCBS.WpfApp.Messages;
using MCBS.WpfApp.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace MCBS.WpfApp.ViewModels.Home
{
    public partial class InstanceResourceViewModel : LaunchViewModel, IRecipient<MinecraftInstanceReloadedMessage>
    {
        public InstanceResourceViewModel(
            PageNavigateCommand pageNavigateCommand,
            IInstanceListStorage instanceListStorage,
            IViewModelFactory<ClientInstanceResourceViewModel> viewModelFactory,
            ILogger<InstanceResourceViewModel> logger) : base(pageNavigateCommand, instanceListStorage, logger)
        {
            ArgumentNullException.ThrowIfNull(logger, nameof(logger));

            _logger = logger;
            _ViewModelFactory = viewModelFactory;

            WeakReferenceMessenger.Default.Register<MinecraftInstanceReloadedMessage>(this);
        }
        
        private readonly ILogger<InstanceResourceViewModel> _logger;
        private readonly IViewModelFactory<ClientInstanceResourceViewModel> _ViewModelFactory;
        private readonly Dictionary<string, ClientInstanceResourceViewModel> _clientResourceCache = [];

        protected override string PageToken => nameof(Pages.Home.InstanceResourcePage);

        [ObservableProperty]
        public partial ClientInstanceResourceViewModel? ClientResource { get; set; }

        protected override async void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.PropertyName == nameof(CurrentInstance) && CurrentInstance is not null)
            {
                if (!_clientResourceCache.TryGetValue(CurrentInstance.InstanceName, out var clientResource))
                {
                    clientResource = _ViewModelFactory.Create(CurrentInstance.InstanceName);
                    _clientResourceCache[CurrentInstance.InstanceName] = clientResource;
                }

                ClientResource = clientResource;
                IAsyncRelayCommand refreshCommand = clientResource.RefreshCommand;
                if (refreshCommand.CanExecute(null) && !refreshCommand.IsRunning)
                    await refreshCommand.ExecuteAsync(null);
            }
        }

        async void IRecipient<MinecraftInstanceReloadedMessage>.Receive(MinecraftInstanceReloadedMessage message)
        {
            if (CurrentInstance is null || CurrentInstance.InstanceName != message.InstanceName)
                return;

            IAsyncRelayCommand? refreshCommand = ClientResource?.RefreshCommand;
            if (refreshCommand is not null && refreshCommand.CanExecute(null) && !refreshCommand.IsRunning)
                await refreshCommand.ExecuteAsync(null);
        }

        [RelayCommand]
        public async Task RefreshList()
        {
            await SaveCurrentInstance();
            await Reload();
        }
    }
}
