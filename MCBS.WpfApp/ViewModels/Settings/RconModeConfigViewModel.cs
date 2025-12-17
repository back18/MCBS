using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MCBS.Config.Minecraft;
using MCBS.WpfApp.Config;
using MCBS.WpfApp.Messages;
using MCBS.WpfApp.Services;
using Microsoft.Extensions.DependencyInjection;
using QuanLib.Core.Events;
using QuanLib.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MCBS.WpfApp.ViewModels.Settings
{
    public partial class RconModeConfigViewModel : ConfigServiceViewModel
    {
        public RconModeConfigViewModel(IMessageBoxService messageBoxService, [FromKeyedServices(typeof(RconModeConfig))] IConfigService configService) : base(messageBoxService)
        {
            ArgumentNullException.ThrowIfNull(configService, nameof(configService));

            _configService = configService;
            var model = (RconModeConfig.Model)configService.GetCurrentConfig();
            Port = model.Port;
            Password = model.Password;

            PropertyChanged += ObservablePropertyChanged;

            ValidateAllProperties();

            WeakReferenceMessenger.Default.Register<PageNavigatingFromMessage, string>(this, nameof(RconModeConfig));
            WeakReferenceMessenger.Default.Register<MainWindowClosingMessage>(this);
        }

        private readonly IConfigService _configService;

        protected override IConfigService? ConfigService
        {
            get => _configService;
            set => throw new NotSupportedException();
        }

        public event EventHandler<EventArgs<object>>? Loaded;

        [ObservableProperty]
        [Range(ushort.MinValue, ushort.MaxValue, ErrorMessage = ErrorMessageHelper.RangeAttribute)]
        public partial int Port { get; set; }

        [ObservableProperty]
        [Required(ErrorMessage = ErrorMessageHelper.RequiredAttribute)]
        public partial string Password { get; set; }

        [RelayCommand]
        public void Load()
        {
            Loaded?.Invoke(this, new(_configService.GetCurrentConfig()));
        }

        [RelayCommand]
        public Task Save()
        {
            return HandleSaveAsync();
        }
    }
}
