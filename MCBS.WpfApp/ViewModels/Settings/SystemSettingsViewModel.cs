using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MCBS.Config;
using MCBS.WpfApp.Config;
using MCBS.WpfApp.Config.Extensions;
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
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace MCBS.WpfApp.ViewModels.Settings
{
    public partial class SystemSettingsViewModel : ConfigServiceViewModel
    {
        public SystemSettingsViewModel(IMessageBoxService messageBoxService, [FromKeyedServices(typeof(SystemConfig))] IConfigStorage configStorage) : base(messageBoxService)
        {
            ArgumentNullException.ThrowIfNull(configStorage, nameof(configStorage));

            _configStorage = configStorage;
            var model = (SystemConfig.Model)configStorage.GetModel().CreateDefault();

            UpdateFromModel(model);

            WeakReferenceMessenger.Default.Register<PageNavigatingFromMessage, string>(this, nameof(SystemConfig));
            WeakReferenceMessenger.Default.Register<MainWindowClosingMessage>(this);
        }

        private readonly IConfigStorage _configStorage;

        protected override IConfigService? ConfigService { get; set; }

        public override bool IsLoaded { get; protected set; }

        public override event EventHandler<EventArgs<object>>? Loaded;

        [ObservableProperty]
        public partial bool AutoRestart { get; set; }

        [ObservableProperty]
        public partial bool BuildColorMappingCaches { get; set; }

        [ObservableProperty]
        public partial bool EnableCompressionCache { get; set; }

        [ObservableProperty]
        public partial bool LoadDllAppComponents { get; set; }

        [ObservableProperty]
        [Required(ErrorMessage = ErrorMessageHelper.RequiredAttribute)]
        public partial ObservableCollection<string> SystemAppComponents { get; set; }

        [ObservableProperty]
        [Required(ErrorMessage = ErrorMessageHelper.RequiredAttribute)]
        public partial string ServicesAppId { get; set; }

        [ObservableProperty]
        [Required(ErrorMessage = ErrorMessageHelper.RequiredAttribute)]
        public partial ObservableCollection<string> StartupChecklist { get; set; }

        [MemberNotNull([nameof(SystemAppComponents), nameof(ServicesAppId), nameof(StartupChecklist)])]
        private void UpdateFromModel(SystemConfig.Model model)
        {
            AutoRestart = model.AutoRestart;
            BuildColorMappingCaches = model.BuildColorMappingCaches;
            EnableCompressionCache = model.EnableCompressionCache;
            LoadDllAppComponents = model.LoadDllAppComponents;
            SystemAppComponents = new ObservableCollection<string>(model.SystemAppComponents);
            ServicesAppId = model.ServicesAppId;
            StartupChecklist = new ObservableCollection<string>(model.StartupChecklist);
        }

        [RelayCommand]
        public async Task Load()
        {
            if (ConfigService is not null)
                return;

            ConfigService = await _configStorage.LoadOrCreateConfigAsync(true);
            var model = (SystemConfig.Model)ConfigService.GetCurrentConfig();

            UpdateFromModel(model);
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

        partial void OnSystemAppComponentsChanged(ObservableCollection<string> oldValue, ObservableCollection<string> newValue)
        {
            oldValue?.CollectionChanged -= CollectionChanged;
            HandleCollectionChanged(nameof(SystemAppComponents), SystemAppComponents);
            newValue?.CollectionChanged += CollectionChanged;
        }

        partial void OnStartupChecklistChanged(ObservableCollection<string> oldValue, ObservableCollection<string> newValue)
        {
            oldValue?.CollectionChanged -= CollectionChanged;
            HandleCollectionChanged(nameof(StartupChecklist), StartupChecklist);
            newValue?.CollectionChanged += CollectionChanged;
        }

        private void CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (ReferenceEquals(sender, SystemAppComponents))
                HandleCollectionChanged(nameof(SystemAppComponents), SystemAppComponents);
            else if (ReferenceEquals(sender, StartupChecklist))
                HandleCollectionChanged(nameof(StartupChecklist), StartupChecklist);
        }
    }
}
