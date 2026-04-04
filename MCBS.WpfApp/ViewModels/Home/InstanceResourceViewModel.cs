using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MCBS.WpfApp.Commands;
using MCBS.WpfApp.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace MCBS.WpfApp.ViewModels.Home
{
    public partial class InstanceResourceViewModel : LaunchViewModel
    {
        public InstanceResourceViewModel(
            PageNavigateCommand pageNavigateCommand,
            IInstanceListStorage instanceListStorage,
            ILogger<InstanceResourceViewModel> logger) : base(pageNavigateCommand, instanceListStorage, logger)
        {
            ArgumentNullException.ThrowIfNull(logger, nameof(logger));

            _logger = logger;
        }

        private readonly ILogger<InstanceResourceViewModel> _logger;
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
                    clientResource = new ClientInstanceResourceViewModel(CurrentInstance);
                    _clientResourceCache[CurrentInstance.InstanceName] = clientResource;
                }

                ClientResource = clientResource;
                IAsyncRelayCommand refreshCommand = clientResource.RefreshCommand;
                if (refreshCommand.CanExecute(null) && !refreshCommand.IsRunning)
                    await refreshCommand.ExecuteAsync(null);
            }
        }

        [RelayCommand]
        public async Task RefreshList()
        {
            await SaveCurrentInstance();
            await Reload();
        }
    }
}
