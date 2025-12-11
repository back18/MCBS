using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MCBS.Config.Constants;
using MCBS.Config.Minecraft;
using MCBS.WpfApp.Config;
using MCBS.WpfApp.Config.Extensions;
using MCBS.WpfApp.Pages.Settings;
using QuanLib.Core;
using QuanLib.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text;
using Page = iNKORE.UI.WPF.Modern.Controls.Page;

namespace MCBS.WpfApp.ViewModels.Settings
{
    public partial class MinecraftSettingsViewModel : ObservableValidator
    {
        private const string McapiModeConfigIdentifier = nameof(MinecraftConfig.McapiModeConfig);
        private const string RconModeConfigIdentifier = nameof(MinecraftConfig.RconModeConfig);
        private const string ConsoleModeConfigIdentifier = nameof(MinecraftConfig.ConsoleModeConfig);

        static MinecraftSettingsViewModel()
        {
            _lazyProperties = new Lazy<ReadOnlyDictionary<string, PropertyInfo>>(
                () => ReflectionHelper.GetObservableProperties(typeof(MinecraftSettingsViewModel)).AsReadOnly(),
                LazyThreadSafetyMode.ExecutionAndPublication
            );
        }

        public MinecraftSettingsViewModel(NavigationService navigationService, IConfigService configService)
        {
            ArgumentNullException.ThrowIfNull(navigationService, nameof(navigationService));
            ArgumentNullException.ThrowIfNull(configService, nameof(configService));

            _navigationService = navigationService;
            _configService = configService;

            var model = (MinecraftConfig.Model)configService.GetCurrentConfig();
            MinecraftPath = model.MinecraftPath;
            MinecraftVersion = model.MinecraftVersion;
            IsServer = model.IsServer;
            ServerAddress = model.ServerAddress;
            ServerPort = model.ServerPort;
            Language = model.Language;
            ResourcePackList = new ObservableCollection<string>(model.ResourcePackList);
            DownloadSource = model.DownloadSource;
            CommunicationMode = model.CommunicationMode;

            _mcapiModeConfigService = configService.CreateSubservices(model.McapiModeConfig);
            _rconModeConfigService = configService.CreateSubservices(model.RconModeConfig);
            _consoleModeConfigService = configService.CreateSubservices(model.ConsoleModeConfig);
            _pageCreateFactory = new PageCreateFactory();

            ResourcePackList.CollectionChanged += CollectionChanged;
            PropertyChanged += ObservablePropertyChanged;

            ValidateAllProperties();
        }

        private readonly NavigationService _navigationService;

        private readonly IConfigService _configService;

        private readonly IConfigService _mcapiModeConfigService;

        private readonly IConfigService _rconModeConfigService;

        private readonly IConfigService _consoleModeConfigService;

        private readonly IPageCreateFactory _pageCreateFactory;

        private static readonly Lazy<ReadOnlyDictionary<string, PropertyInfo>> _lazyProperties;

        private static ReadOnlyDictionary<string, PropertyInfo> Properties => _lazyProperties.Value;

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

        [Required(ErrorMessage = ErrorMessageHelper.RequiredAttribute)]
        public ObservableCollection<string> ResourcePackList { get; set; }

        [ObservableProperty]
        [Required(ErrorMessage = ErrorMessageHelper.RequiredAttribute)]
        [NewAllowedValues(DownloadSources.MOJANG, DownloadSources.BMCLAPI, ErrorMessage = ErrorMessageHelper.NewAllowedValuesAttribute)]
        public partial string DownloadSource { get; set; }

        [ObservableProperty]
        [Required(ErrorMessage = ErrorMessageHelper.RequiredAttribute)]
        [NewAllowedValues(CommunicationModes.MCAPI, CommunicationModes.RCON, CommunicationModes.CONSOLE, CommunicationModes.HYBRID, ErrorMessage = ErrorMessageHelper.NewAllowedValuesAttribute)]
        [AllowedValuesIf(nameof(IsServer), CompareOperator.Equal, false, CommunicationModes.MCAPI)]
        public partial string CommunicationMode { get; set; }

        [RelayCommand]
        public async Task Save()
        {
            if (_configService.IsModified)
                await _configService.GetConfigStorage().SaveConfigAsync();
        }

        [RelayCommand]
        public void NavigateToSubconfig(object? parameter)
        {
            if (parameter is not string identifier)
                return;

            Type pageType;
            IConfigService configService;
            switch (identifier)
            {
                case McapiModeConfigIdentifier:
                    pageType = typeof(McapiModeConfigPage);
                    configService = _mcapiModeConfigService;
                    break;
                case RconModeConfigIdentifier:
                    pageType = typeof(RconModeConfigPage);
                    configService = _rconModeConfigService;
                    break;
                case ConsoleModeConfigIdentifier:
                    pageType = typeof(ConsoleModeConfigPage);
                    configService = _consoleModeConfigService;
                    break;
                default:
                    return;
            }

            Page page = _pageCreateFactory.GetOrCreatePage(pageType, configService);
            _navigationService.Navigate(page);
        }

        private void ObservablePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            string? propertyName = e.PropertyName;
            if (!string.IsNullOrEmpty(propertyName) && Properties.TryGetValue(propertyName, out var propertyInfo))
            {
                ValidateAllProperties();
                if (!HasErrors)
                {
                    object? value = propertyInfo.GetValue(this);
                    _configService.SetPropertyValue(propertyName, value);
                }
            }
        }

        private void CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (ReferenceEquals(sender, ResourcePackList))
                HandleCollectionChanged(nameof(ResourcePackList), ResourcePackList);
        }

        private void HandleCollectionChanged(string propertyName, ObservableCollection<string> collection)
        {
            ArgumentException.ThrowIfNullOrEmpty(propertyName, nameof(propertyName));
            ArgumentNullException.ThrowIfNull(collection, nameof(collection));

            ValidateProperty(collection, propertyName);
            if (!HasErrors)
                _configService.SetPropertyValue(propertyName, collection.ToArray());
        }
    }
}
