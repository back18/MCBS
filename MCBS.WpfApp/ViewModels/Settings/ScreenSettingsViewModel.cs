using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MCBS.Config;
using MCBS.WpfApp.Config.Extensions;
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
    public partial class ScreenSettingsViewModel : ConfigSettingsViewModel
    {
        public ScreenSettingsViewModel(ILoggerFactory loggerFactory, IMessageBoxService messageBoxService, [FromKeyedServices(typeof(ScreenConfig))] IConfigStorage configStorage) : base(loggerFactory, messageBoxService)
        {
            ArgumentNullException.ThrowIfNull(configStorage, nameof(configStorage));

            _configStorage = configStorage;

            object model = configStorage.GetModel().CreateDefault();
            UpdateFromModel(model);

            WeakReferenceMessenger.Default.Register<PageNavigatingFromMessage, string>(this, nameof(ScreenConfig));
            WeakReferenceMessenger.Default.Register<MainWindowClosingMessage>(this);
        }

        private readonly IConfigStorage _configStorage;

        protected override IConfigService? ConfigService { get; set; }

        public override bool IsLoaded { get; protected set; }

        public override event EventHandler<EventArgs<object>>? Loaded;

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

        [ObservableProperty]
        [Required(ErrorMessage = ErrorMessageHelper.RequiredAttribute)]
        public partial ObservableCollection<string> ScreenOperatorList { get; set; }

        [ObservableProperty]
        [Required(ErrorMessage = ErrorMessageHelper.RequiredAttribute)]
        public partial ObservableCollection<string> ScreenBuildOperatorList { get; set; }

        [ObservableProperty]
        [Required(ErrorMessage = ErrorMessageHelper.RequiredAttribute)]
        public partial ObservableCollection<string> ScreenBlockBlacklist { get; set; }

        [MemberNotNull([
            nameof(RightClickObjective),
            nameof(RightClickCriterion),
            nameof(RightClickItemId),
            nameof(TextEditorItemId),
            nameof(ScreenBuilderItemName),
            nameof(ScreenOperatorList),
            nameof(ScreenBuildOperatorList),
            nameof(ScreenBlockBlacklist)])]
        protected override void UpdateFromModel(object model)
        {
            if (model is not ScreenConfig.Model typedModel)
                throw new ArgumentException($"Model must be of type {typeof(ScreenConfig.Model).FullName}", nameof(model));

            MaxCount = typedModel.MaxCount;
            MinLength = typedModel.MinLength;
            MaxLength = typedModel.MaxLength;
            MinAltitude = typedModel.MinAltitude;
            MaxAltitude = typedModel.MaxAltitude;
            InitialWidth = typedModel.InitialWidth;
            InitialHeight = typedModel.InitialHeight;
            ScreenIdleTimeout = typedModel.ScreenIdleTimeout;
            RightClickObjective = typedModel.RightClickObjective;
            RightClickCriterion = typedModel.RightClickCriterion;
            RightClickItemId = typedModel.RightClickItemId;
            TextEditorItemId = typedModel.TextEditorItemId;
            ScreenBuilderItemName = typedModel.ScreenBuilderItemName;
            ScreenOperatorList = new ObservableCollection<string>(typedModel.ScreenOperatorList);
            ScreenBuildOperatorList = new ObservableCollection<string>(typedModel.ScreenBuildOperatorList);
            ScreenBlockBlacklist = new ObservableCollection<string>(typedModel.ScreenBlockBlacklist);
        }

        [RelayCommand]
        public async Task Load()
        {
            if (ConfigService is not null)
                return;

            ConfigService = await _configStorage.LoadOrCreateConfigAsync(true);
            object model = ConfigService.GetCurrentConfig();

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

        partial void OnScreenOperatorListChanged(ObservableCollection<string> oldValue, ObservableCollection<string> newValue)
        {
            oldValue?.CollectionChanged -= CollectionChanged;
            HandleCollectionChanged(nameof(ScreenOperatorList), ScreenOperatorList);
            newValue?.CollectionChanged += CollectionChanged;
        }

        partial void OnScreenBuildOperatorListChanged(ObservableCollection<string> oldValue, ObservableCollection<string> newValue)
        {
            oldValue?.CollectionChanged -= CollectionChanged;
            HandleCollectionChanged(nameof(ScreenBuildOperatorList), ScreenBuildOperatorList);
            newValue?.CollectionChanged += CollectionChanged;
        }

        partial void OnScreenBlockBlacklistChanged(ObservableCollection<string> oldValue, ObservableCollection<string> newValue)
        {
            oldValue?.CollectionChanged -= CollectionChanged;
            HandleCollectionChanged(nameof(ScreenBlockBlacklist), ScreenBlockBlacklist);
            newValue?.CollectionChanged += CollectionChanged;
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
    }
}
