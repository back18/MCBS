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
    public partial class ScreenSettingsViewModel : ObservableValidator
    {
        static ScreenSettingsViewModel()
        {
            _lazyProperties = new Lazy<ReadOnlyDictionary<string, PropertyInfo>>(
                () => ReflectionHelper.GetObservableProperties(typeof(ScreenSettingsViewModel)).AsReadOnly(),
                LazyThreadSafetyMode.ExecutionAndPublication
            );
        }

        public ScreenSettingsViewModel(IConfigService configService)
        {
            ArgumentNullException.ThrowIfNull(configService, nameof(configService));

            _configService = configService;
            var model = (ScreenConfig.Model)configService.GetCurrentConfig();
            MaxCount = model.MaxCount;
            MinLength = model.MinLength;
            MaxLength = model.MaxLength;
            MinAltitude = model.MinAltitude;
            MaxAltitude = model.MaxAltitude;
            InitialWidth = model.InitialWidth;
            InitialHeight = model.InitialHeight;
            ScreenIdleTimeout = model.ScreenIdleTimeout;
            RightClickObjective = model.RightClickObjective;
            RightClickCriterion = model.RightClickCriterion;
            RightClickItemId = model.RightClickItemId;
            TextEditorItemId = model.TextEditorItemId;
            ScreenBuilderItemName = model.ScreenBuilderItemName;
            ScreenOperatorList = new ObservableCollection<string>(model.ScreenOperatorList);
            ScreenBuildOperatorList = new ObservableCollection<string>(model.ScreenBuildOperatorList);
            ScreenBlockBlacklist = new ObservableCollection<string>(model.ScreenBlockBlacklist);

            ScreenOperatorList.CollectionChanged += CollectionChanged;
            ScreenBuildOperatorList.CollectionChanged += CollectionChanged;
            ScreenBlockBlacklist.CollectionChanged += CollectionChanged;
            PropertyChanged += ObservablePropertyChanged;

            ValidateAllProperties();
        }

        private readonly IConfigService _configService;

        private static readonly Lazy<ReadOnlyDictionary<string, PropertyInfo>> _lazyProperties;

        private static ReadOnlyDictionary<string, PropertyInfo> Properties => _lazyProperties.Value;

        [ObservableProperty]
        [Range(0, 64, ErrorMessage = ErrorMessageHelper.RangeAttribute)]
        public partial int MaxCount { get; set; }

        [ObservableProperty]
        [Range(1, 512, ErrorMessage = ErrorMessageHelper.RangeAttribute)]
        public partial int MinLength { get; set; }

        [ObservableProperty]
        [Range(1, 512, ErrorMessage = ErrorMessageHelper.RangeAttribute)]
        [GreaterThan(nameof(MinLength), ErrorMessage = ErrorMessageHelper.GreaterThanAttribute)]
        public partial int MaxLength { get; set; }

        [ObservableProperty]
        [Range(-64, 319, ErrorMessage = ErrorMessageHelper.RangeAttribute)]
        public partial int MinAltitude { get; set; }

        [ObservableProperty]
        [Range(-64, 319, ErrorMessage = ErrorMessageHelper.RangeAttribute)]
        [GreaterThan(nameof(MinAltitude), ErrorMessage = ErrorMessageHelper.GreaterThanAttribute)]
        public partial int MaxAltitude { get; set; }

        [ObservableProperty]
        [Range(1, 512, ErrorMessage = ErrorMessageHelper.RangeAttribute)]
        public partial int InitialWidth { get; set; }

        [ObservableProperty]
        [Range(1, 512, ErrorMessage = ErrorMessageHelper.RangeAttribute)]
        public partial int InitialHeight { get; set; }

        [ObservableProperty]
        [Range(-1, int.MaxValue, ErrorMessage = ErrorMessageHelper.RangeAttribute)]
        public partial int ScreenIdleTimeout { get; set; }

        [ObservableProperty]
        [Required(ErrorMessage = ErrorMessageHelper.RequiredAttribute)]
        public partial string RightClickObjective { get; set; }

        [ObservableProperty]
        [Required(ErrorMessage = ErrorMessageHelper.RequiredAttribute)]
        public partial string RightClickCriterion { get; set; }

        [ObservableProperty]
        [Required(ErrorMessage = ErrorMessageHelper.RequiredAttribute)]
        public partial string RightClickItemId { get; set; }

        [ObservableProperty]
        [Required(ErrorMessage = ErrorMessageHelper.RequiredAttribute)]
        public partial string TextEditorItemId { get; set; }

        [ObservableProperty]
        [Required(ErrorMessage = ErrorMessageHelper.RequiredAttribute)]
        public partial string ScreenBuilderItemName { get; set; }

        [Required(ErrorMessage = ErrorMessageHelper.RequiredAttribute)]
        public ObservableCollection<string> ScreenOperatorList { get; }

        [Required(ErrorMessage = ErrorMessageHelper.RequiredAttribute)]
        public ObservableCollection<string> ScreenBuildOperatorList { get; }

        [Required(ErrorMessage = ErrorMessageHelper.RequiredAttribute)]
        public ObservableCollection<string> ScreenBlockBlacklist { get; }

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
            if (ReferenceEquals(sender, ScreenOperatorList))
                HandleCollectionChanged(nameof(ScreenOperatorList), ScreenOperatorList);
            else if (ReferenceEquals(sender, ScreenBuildOperatorList))
                HandleCollectionChanged(nameof(ScreenBuildOperatorList), ScreenBuildOperatorList);
            else if (ReferenceEquals(sender, ScreenBlockBlacklist))
                HandleCollectionChanged(nameof(ScreenBlockBlacklist), ScreenBlockBlacklist);
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
