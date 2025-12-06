using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MCBS.Config.Minecraft;
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
    public partial class ConsoleModeConfigViewModel : ObservableValidator
    {
        static ConsoleModeConfigViewModel()
        {
            _lazyProperties = new Lazy<ReadOnlyDictionary<string, PropertyInfo>>(
                () => ReflectionHelper.GetObservableProperties(typeof(ConsoleModeConfigViewModel)).AsReadOnly(),
                LazyThreadSafetyMode.ExecutionAndPublication
            );
        }

        public ConsoleModeConfigViewModel(IConfigService configService)
        {
            ArgumentNullException.ThrowIfNull(configService, nameof(configService));
            _configService = configService;
            var model = (ConsoleModeConfig.Model)configService.GetCurrentConfig();

            JavaPath = model.JavaPath;
            LaunchArguments = model.LaunchArguments;
            MclogRegexFilter = new ObservableCollection<string>(model.MclogRegexFilter);

            PropertyChanged += ObservablePropertyChanged;

            ValidateAllProperties();
        }

        private readonly IConfigService _configService;

        private static readonly Lazy<ReadOnlyDictionary<string, PropertyInfo>> _lazyProperties;

        private static ReadOnlyDictionary<string, PropertyInfo> Properties => _lazyProperties.Value;

        [ObservableProperty]
        [Required(ErrorMessage = ErrorMessageHelper.RequiredAttribute)]
        public partial string JavaPath { get; set; }

        [ObservableProperty]
        [Required(ErrorMessage = ErrorMessageHelper.RequiredAttribute)]
        public partial string LaunchArguments { get; set; }

        [Required(ErrorMessage = ErrorMessageHelper.RequiredAttribute)]
        public ObservableCollection<string> MclogRegexFilter { get; set; }

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
            if (ReferenceEquals(sender, MclogRegexFilter))
                HandleCollectionChanged(nameof(MclogRegexFilter), MclogRegexFilter);
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
