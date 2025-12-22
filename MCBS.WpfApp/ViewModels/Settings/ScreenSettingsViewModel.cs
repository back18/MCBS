using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MCBS.Config;
using MCBS.WpfApp.Config;
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
    public partial class ScreenSettingsViewModel : ConfigServiceViewModel
    {
        public ScreenSettingsViewModel(ILoggerFactory loggerFactory, IMessageBoxService messageBoxService, [FromKeyedServices(typeof(ScreenConfig))] IConfigStorage configStorage) : base(loggerFactory, messageBoxService)
        {
            ArgumentNullException.ThrowIfNull(configStorage, nameof(configStorage));

            _configStorage = configStorage;
            var model = (ScreenConfig.Model)configStorage.GetModel().CreateDefault();

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
        private void UpdateFromModel(ScreenConfig.Model model)
        {
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
        }

        [RelayCommand]
        public async Task Load()
        {
            if (ConfigService is not null)
                return;

            ConfigService = await _configStorage.LoadOrCreateConfigAsync(true);
            var model = (ScreenConfig.Model)ConfigService.GetCurrentConfig();

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
