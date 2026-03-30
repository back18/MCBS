using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MCBS.Config.Minecraft;
using MCBS.WpfApp.Messages;
using MCBS.WpfApp.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuanLib.Core.Events;
using QuanLib.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace MCBS.WpfApp.ViewModels.Settings
{
    public partial class McapiModeConfigViewModel : ConfigSettingsViewModel
    {
        public McapiModeConfigViewModel(ILoggerFactory loggerFactory, IMessageBoxService messageBoxService, [FromKeyedServices(typeof(McapiModeConfig))] IConfigService configService) : base(loggerFactory, messageBoxService)
        {
            ArgumentNullException.ThrowIfNull(configService, nameof(configService));

            _configService = configService;
            UpdateFromModel(configService.GetCurrentConfig());

            WeakReferenceMessenger.Default.Register<PageNavigatingFromMessage, string>(this, nameof(McapiModeConfig));
            WeakReferenceMessenger.Default.Register<MainWindowClosingMessage>(this);
        }

        private readonly IConfigService _configService;

        protected override IConfigService? ConfigService
        {
            get => _configService;
            set => throw new NotSupportedException();
        }

        public override bool IsLoaded { get; protected set; }

        public override event EventHandler<EventArgs<object>>? Loaded;

        [ObservableProperty]
        [Required(ErrorMessage = ErrorMessageHelper.RequiredAttribute)]
        public partial string Address { get; set; }

        [ObservableProperty]
        [Range(ushort.MinValue, ushort.MaxValue, ErrorMessage = ErrorMessageHelper.RangeAttribute)]
        public partial int Port { get; set; }

        [ObservableProperty]
        [Required(ErrorMessage = ErrorMessageHelper.RequiredAttribute)]
        public partial string Password { get; set; }

        [MemberNotNull(nameof(Address), nameof(Password))]
        protected override void UpdateFromModel(object model)
        {
            if (model is not McapiModeConfig.Model typedModel)
                throw new ArgumentException($"Model must be of type {typeof(McapiModeConfig.Model).FullName}", nameof(model));

            Address = typedModel.Address;
            Port = typedModel.Port;
            Password = typedModel.Password;
        }

        [RelayCommand]
        public void Load()
        {
            object model = _configService.GetCurrentConfig();
            UpdateFromModel(model);
            if (!IsLoaded)
                PropertyChanged += ObservablePropertyChanged;

            ValidateAllProperties();
            IsLoaded = true;
            Loaded?.Invoke(this, new(model));
        }

        [RelayCommand]
        public Task Save()
        {
            return HandleSaveAsync();
        }
    }
}
