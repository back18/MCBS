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
    public partial class RconModeConfigViewModel : ConfigSettingsViewModel
    {
        public RconModeConfigViewModel(ILoggerFactory loggerFactory, IMessageBoxService messageBoxService, [FromKeyedServices(typeof(RconModeConfig))] IConfigService configService) : base(loggerFactory, messageBoxService)
        {
            ArgumentNullException.ThrowIfNull(configService, nameof(configService));

            _configService = configService;
            UpdateFromModel(configService.GetCurrentConfig());

            WeakReferenceMessenger.Default.Register<PageNavigatingFromMessage, string>(this, nameof(RconModeConfig));
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
        [Range(ushort.MinValue, ushort.MaxValue, ErrorMessage = ErrorMessageHelper.RangeAttribute)]
        public partial int Port { get; set; }

        [ObservableProperty]
        [Required(ErrorMessage = ErrorMessageHelper.RequiredAttribute)]
        public partial string Password { get; set; }

        [MemberNotNull(nameof(Password))]
        protected override void UpdateFromModel(object model)
        {
            var typedModel = (RconModeConfig.Model)model;
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
