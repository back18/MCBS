using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MCBS.Config;
using MCBS.WpfApp.Config;
using QuanLib.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text;

namespace MCBS.WpfApp.ViewModels.Settings
{
    public partial class SystemSettingsViewModel : ObservableValidator
    {
        static SystemSettingsViewModel()
        {
            _lazyProperties = new Lazy<ReadOnlyDictionary<string, PropertyInfo>>(
                () => ReflectionHelper.GetObservableProperties(typeof(SystemSettingsViewModel)).AsReadOnly(),
                LazyThreadSafetyMode.ExecutionAndPublication
            );
        }

        public SystemSettingsViewModel(IConfigService configService)
        {
            ArgumentNullException.ThrowIfNull(configService, nameof(configService));

            _configService = configService;
            var model = (SystemConfig.Model)configService.GetCurrentConfig();
            AutoRestart = model.AutoRestart;
            BuildColorMappingCaches = model.BuildColorMappingCaches;
            EnableCompressionCache = model.EnableCompressionCache;
            LoadDllAppComponents = model.LoadDllAppComponents;
            SystemAppComponents = new ObservableCollection<string>(model.SystemAppComponents);
            ServicesAppId = model.ServicesAppId;
            StartupChecklist = new ObservableCollection<string>(model.StartupChecklist);

            SystemAppComponents.CollectionChanged += CollectionChanged;
            StartupChecklist.CollectionChanged += CollectionChanged;
            PropertyChanged += ObservablePropertyChanged;

            ValidateAllProperties();
        }

        private readonly IConfigService _configService;

        private static readonly Lazy<ReadOnlyDictionary<string, PropertyInfo>> _lazyProperties;

        private static ReadOnlyDictionary<string, PropertyInfo> Properties => _lazyProperties.Value;

        [ObservableProperty]
        public partial bool AutoRestart { get; set; }

        [ObservableProperty]
        public partial bool BuildColorMappingCaches { get; set; }

        [ObservableProperty]
        public partial bool EnableCompressionCache { get; set; }

        [ObservableProperty]
        public partial bool LoadDllAppComponents { get; set; }

        [Required(ErrorMessage = ErrorMessageHelper.RequiredAttribute)]
        public ObservableCollection<string> SystemAppComponents { get; }

        [ObservableProperty]
        [Required(ErrorMessage = ErrorMessageHelper.RequiredAttribute)]
        public partial string ServicesAppId { get; set; }

        [Required(ErrorMessage = ErrorMessageHelper.RequiredAttribute)]
        public ObservableCollection<string> StartupChecklist { get; }

        [RelayCommand]
        public async Task Save()
        {
            if (_configService.IsModified)
                await _configService.GetConfigStorage().SaveConfigAsync();
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
            if (ReferenceEquals(sender, SystemAppComponents))
                HandleCollectionChanged(nameof(SystemAppComponents), SystemAppComponents);
            else if (ReferenceEquals(sender, StartupChecklist))
                HandleCollectionChanged(nameof(StartupChecklist), StartupChecklist);
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
