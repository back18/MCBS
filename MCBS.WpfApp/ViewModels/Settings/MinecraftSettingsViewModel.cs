using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MCBS.Config.Constants;
using MCBS.Config.Minecraft;
using MCBS.WpfApp.Config;
using MCBS.WpfApp.Config.Extensions;
using MCBS.WpfApp.Messages;
using MCBS.WpfApp.Pages.Settings;
using MCBS.WpfApp.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuanLib.Core;
using QuanLib.Core.Events;
using QuanLib.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Page = iNKORE.UI.WPF.Modern.Controls.Page;

namespace MCBS.WpfApp.ViewModels.Settings
{
    public partial class MinecraftSettingsViewModel : ConfigServiceViewModel
    {
        private const string McapiModeConfigIdentifier = nameof(MinecraftConfig.McapiModeConfig);
        private const string RconModeConfigIdentifier = nameof(MinecraftConfig.RconModeConfig);
        private const string ConsoleModeConfigIdentifier = nameof(MinecraftConfig.ConsoleModeConfig);

        public MinecraftSettingsViewModel(
            ILoggerFactory loggerFactory,
            IServiceProvider serviceProvider,
            INavigable navigable,
            IMessageBoxService messageBoxService,
            [FromKeyedServices(typeof(MinecraftConfig))] IConfigStorage configStorage) : base(loggerFactory, messageBoxService)
        {
            ArgumentNullException.ThrowIfNull(serviceProvider, nameof(serviceProvider));
            ArgumentNullException.ThrowIfNull(navigable, nameof(navigable));
            ArgumentNullException.ThrowIfNull(messageBoxService, nameof(messageBoxService));
            ArgumentNullException.ThrowIfNull(configStorage, nameof(configStorage));

            _serviceProvider = serviceProvider;
            _navigable = navigable;
            _configStorage = configStorage;
            var model = (MinecraftConfig.Model)configStorage.GetModel().CreateDefault();

            UpdateFromModel(model);

            WeakReferenceMessenger.Default.Register<PageNavigatingFromMessage, string>(this, nameof(MinecraftConfig));
            WeakReferenceMessenger.Default.Register<MainWindowClosingMessage>(this);
        }

        private readonly IServiceProvider _serviceProvider;

        private readonly INavigable _navigable;

        private readonly IConfigStorage _configStorage;

        protected override IConfigService? ConfigService { get; set; }

        public override bool IsLoaded { get; protected set; }

        public override event EventHandler<EventArgs<object>>? Loaded;

        [ObservableProperty]
        [Required(ErrorMessage = ErrorMessageHelper.RequiredAttribute)]
        public partial string MinecraftPath { get; set; }

        [ObservableProperty]
        [Required(ErrorMessage = ErrorMessageHelper.RequiredAttribute)]
        public partial string MinecraftVersion { get; set; }

        [ObservableProperty]
        [Required(ErrorMessage = ErrorMessageHelper.RequiredAttribute)]
        public partial bool IsServer { get; set; }

        [ObservableProperty]
        [RequiredIf(nameof(IsServer), CompareOperator.Equal, true, ErrorMessage = ErrorMessageHelper.RequiredIfAttribute)]
        public partial string ServerAddress { get; set; }

        [ObservableProperty]
        [RequiredIf(nameof(IsServer), CompareOperator.Equal, true, ErrorMessage = ErrorMessageHelper.RequiredIfAttribute)]
        [Range(ushort.MinValue, ushort.MaxValue, ErrorMessage = ErrorMessageHelper.RangeAttribute)]
        public partial int ServerPort { get; set; }

        [ObservableProperty]
        [Required(ErrorMessage = ErrorMessageHelper.RequiredAttribute)]
        public partial string Language { get; set; }

        [ObservableProperty]
        [Required(ErrorMessage = ErrorMessageHelper.RequiredAttribute)]
        public partial ObservableCollection<string> ResourcePackList { get; set; }

        [ObservableProperty]
        [Required(ErrorMessage = ErrorMessageHelper.RequiredAttribute)]
        [NewAllowedValues(DownloadSources.MOJANG, DownloadSources.BMCLAPI, ErrorMessage = ErrorMessageHelper.NewAllowedValuesAttribute)]
        public partial string DownloadSource { get; set; }

        [ObservableProperty]
        [Required(ErrorMessage = ErrorMessageHelper.RequiredAttribute)]
        [NewAllowedValues(CommunicationModes.MCAPI, CommunicationModes.RCON, CommunicationModes.CONSOLE, CommunicationModes.HYBRID, ErrorMessage = ErrorMessageHelper.NewAllowedValuesAttribute)]
        [AllowedValuesIf(nameof(IsServer), CompareOperator.Equal, false, CommunicationModes.MCAPI)]
        public partial string CommunicationMode { get; set; }

        [MemberNotNull([
            nameof(MinecraftPath),
            nameof(MinecraftVersion),
            nameof(ServerAddress),
            nameof(Language),
            nameof(ResourcePackList),
            nameof(DownloadSource),
            nameof(CommunicationMode)])]
        private void UpdateFromModel(MinecraftConfig.Model model)
        {
            MinecraftPath = model.MinecraftPath;
            MinecraftVersion = model.MinecraftVersion;
            IsServer = model.IsServer;
            ServerAddress = model.ServerAddress;
            ServerPort = model.ServerPort;
            Language = model.Language;
            ResourcePackList = new ObservableCollection<string>(model.ResourcePackList);
            DownloadSource = model.DownloadSource;
            CommunicationMode = model.CommunicationMode;
        }

        [RelayCommand]
        public async Task Load()
        {
            if (ConfigService is not null)
                return;

            ConfigService = await _configStorage.LoadOrCreateConfigAsync(true);
            var model = (MinecraftConfig.Model)ConfigService.GetCurrentConfig();

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

        [RelayCommand]
        public void NavigateToSubconfig(object? parameter)
        {
            if (parameter is not string identifier)
                return;

            Type? pageType = identifier switch
            {
                McapiModeConfigIdentifier => typeof(McapiModeConfigPage),
                RconModeConfigIdentifier => typeof(RconModeConfigPage),
                ConsoleModeConfigIdentifier => typeof(ConsoleModeConfigPage),
                _ => null,
            };

            if (pageType is null || _serviceProvider.GetService(pageType) is not Page page)
                return;

            _navigable.NavigationService.Navigate(page);
        }

        partial void OnResourcePackListChanged(ObservableCollection<string> oldValue, ObservableCollection<string> newValue)
        {
            oldValue?.CollectionChanged -= CollectionChanged;
            HandleCollectionChanged(nameof(ResourcePackList), ResourcePackList);
            newValue?.CollectionChanged += CollectionChanged;
        }

        private void CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (ReferenceEquals(sender, ResourcePackList))
                HandleCollectionChanged(nameof(ResourcePackList), ResourcePackList);
        }
    }
}
