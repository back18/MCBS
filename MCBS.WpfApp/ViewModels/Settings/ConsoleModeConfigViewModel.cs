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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MCBS.WpfApp.ViewModels.Settings
{
    public partial class ConsoleModeConfigViewModel : ConfigServiceViewModel
    {
        public ConsoleModeConfigViewModel(IMessageBoxService messageBoxService, [FromKeyedServices(typeof(ConsoleModeConfig))] IConfigService configService) : base(messageBoxService)
        {
            ArgumentNullException.ThrowIfNull(configService, nameof(configService));
            _configService = configService;
            var model = (ConsoleModeConfig.Model)configService.GetCurrentConfig();

            JavaPath = model.JavaPath;
            LaunchArguments = model.LaunchArguments;
            MclogRegexFilter = new ObservableCollection<string>(model.MclogRegexFilter);

            PropertyChanged += ObservablePropertyChanged;

            ValidateAllProperties();

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

        [RelayCommand]
        public void Load()
        {
            IsLoaded = true;
            Loaded?.Invoke(this, new(_configService.GetCurrentConfig()));
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
