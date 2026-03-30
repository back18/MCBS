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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace MCBS.WpfApp.ViewModels.Settings
{
    public partial class ConsoleModeConfigViewModel : ConfigSettingsViewModel
    {
        public ConsoleModeConfigViewModel(ILoggerFactory loggerFactory, IMessageBoxService messageBoxService, [FromKeyedServices(typeof(ConsoleModeConfig))] IConfigService configService) : base(loggerFactory, messageBoxService)
        {
            ArgumentNullException.ThrowIfNull(configService, nameof(configService));

            _configService = configService;
            UpdateFromModel(configService.GetCurrentConfig());

            WeakReferenceMessenger.Default.Register<PageNavigatingFromMessage, string>(this, nameof(ConsoleModeConfig));
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
        public partial string JavaPath { get; set; }

        [ObservableProperty]
        [Required(ErrorMessage = ErrorMessageHelper.RequiredAttribute)]
        public partial string LaunchArguments { get; set; }

        [ObservableProperty]
        [Required(ErrorMessage = ErrorMessageHelper.RequiredAttribute)]
        public partial ObservableCollection<string> MclogRegexFilter { get; set; }

        [MemberNotNull([
            nameof(JavaPath),
            nameof(LaunchArguments),
            nameof(MclogRegexFilter)])]
        protected override void UpdateFromModel(object model)
        {
            if (model is not ConsoleModeConfig.Model typedModel)
                throw new ArgumentException($"Model must be of type {typeof(ConsoleModeConfig.Model).FullName}", nameof(model));

            JavaPath = typedModel.JavaPath;
            LaunchArguments = typedModel.LaunchArguments;
            MclogRegexFilter = new ObservableCollection<string>(typedModel.MclogRegexFilter);
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

        partial void OnMclogRegexFilterChanged(ObservableCollection<string> oldValue, ObservableCollection<string> newValue)
        {
            oldValue?.CollectionChanged -= CollectionChanged;
            HandleCollectionChanged(nameof(MclogRegexFilter), MclogRegexFilter);
            newValue?.CollectionChanged += CollectionChanged;
        }

        private void CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (ReferenceEquals(sender, MclogRegexFilter))
                HandleCollectionChanged(nameof(MclogRegexFilter), MclogRegexFilter);
        }
    }
}
