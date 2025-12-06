using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MCBS.Config.Minecraft;
using MCBS.WpfApp.Config;
using QuanLib.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Text;

namespace MCBS.WpfApp.ViewModels.Settings
{
    public partial class McapiModeConfigViewModel : ObservableValidator
    {
        static McapiModeConfigViewModel()
        {
            _lazyProperties = new Lazy<ReadOnlyDictionary<string, PropertyInfo>>(
                () => ReflectionHelper.GetObservableProperties(typeof(McapiModeConfigViewModel)).AsReadOnly(),
                LazyThreadSafetyMode.ExecutionAndPublication
            );
        }

        public McapiModeConfigViewModel(IConfigService configService)
        {
            ArgumentNullException.ThrowIfNull(configService, nameof(configService));

            _configService = configService;
            var model = (McapiModeConfig.Model)configService.GetCurrentConfig();
            Address = model.Address;
            Port = model.Port;
            Password = model.Password;

            PropertyChanged += ObservablePropertyChanged;

            ValidateAllProperties();
        }

        private readonly IConfigService _configService;

        private static readonly Lazy<ReadOnlyDictionary<string, PropertyInfo>> _lazyProperties;

        private static ReadOnlyDictionary<string, PropertyInfo> Properties => _lazyProperties.Value;

        [ObservableProperty]
        [Required(ErrorMessage = ErrorMessageHelper.RequiredAttribute)]
        public partial string Address { get; set; }

        [ObservableProperty]
        [Range(ushort.MinValue, ushort.MaxValue, ErrorMessage = ErrorMessageHelper.RangeAttribute)]
        public partial int Port { get; set; }

        [ObservableProperty]
        [Required(ErrorMessage = ErrorMessageHelper.RequiredAttribute)]
        public partial string Password { get; set; }

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
    }
}
