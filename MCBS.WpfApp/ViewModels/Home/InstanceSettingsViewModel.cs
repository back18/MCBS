using CommunityToolkit.Mvvm.Messaging;
using MCBS.WpfApp.Attributes;
using MCBS.WpfApp.Messages;
using MCBS.WpfApp.Models;
using MCBS.WpfApp.Resources.Strings;
using MCBS.WpfApp.Services;
using MCBS.WpfApp.ViewModels.Settings;
using Microsoft.Extensions.Logging;
using QuanLib.Core;
using QuanLib.DataAnnotations;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Windows;

namespace MCBS.WpfApp.ViewModels.Home
{
    [ExcludeFromDI]
    public partial class InstanceSettingsViewModel : MinecraftSettingsViewModel
    {
        public InstanceSettingsViewModel(
            ILoggerFactory loggerFactory,
            IMessageBoxService messageBoxService,
            IMinecraftSettingsNavigationService navigationService,
            IConfigStorage configStorage,
            IInstanceListStorage instanceListStorage) : base(loggerFactory, messageBoxService, navigationService, configStorage)
        {
            ArgumentNullException.ThrowIfNull(instanceListStorage, nameof(instanceListStorage));

            _instanceListStorage = instanceListStorage;
            _instanceName = Guid.NewGuid().ToString();
        }

        private readonly IInstanceListStorage _instanceListStorage;
        private string _instanceName;

        protected override void UpdateFromModel(object model)
        {
            if (model is not MinecraftInstanceConfig.Model typedModel)
                throw new ArgumentException($"Model must be of type {typeof(MinecraftInstanceConfig.Model).FullName}", nameof(model));

            base.UpdateFromModel(model);
            SetProperty(ref _instanceName, typedModel.InstanceName, true, nameof(InstanceName));
        }

        [ManualObservableProperty]
        [Required(ErrorMessage = ErrorMessageHelper.RequiredAttribute)]
        [FileName]
        public string InstanceName
        {
            get => _instanceName;
            set
            {
                if (value == _instanceName)
                    return;

                ValidateProperty(value);
                if (HasErrors)
                {
                    var errors = GetErrors();
                    ValidationResult? validationResult = errors.FirstOrDefault(s => s.MemberNames.Contains(nameof(InstanceName)));
                    if (validationResult is not null)
                    {
                        _messageBoxService.Show(
                         validationResult.ErrorMessage ?? string.Empty,
                         Lang.MessageBox_Caption_InvalidInstanceName,
                         MessageBoxButton.OK,
                         MessageBoxImage.Warning);
                        ValidateProperty(_instanceName);
                        return;
                    }
                }

                string oldName = _instanceName;
                SetProperty(ref _instanceName, value, true);
                OnInstanceNameChanged(oldName, value);
            }
        }

        protected override void ObservablePropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            base.ObservablePropertyChanged(sender, e);

            if (!string.IsNullOrEmpty(e.PropertyName) && e.PropertyName != nameof(InstanceName))
                WeakReferenceMessenger.Default.Send(new MinecraftInstanceModifiedMessage(InstanceName));
        }

        protected virtual async void OnInstanceNameChanged(string oldValue, string newValue)
        {
            await HandleInstanceRenameAsync(oldValue, newValue);
        }

        private async Task HandleInstanceRenameAsync(string oldName, string newName)
        {
            try
            {
                await HandleSaveAsync();

                bool success = await _instanceListStorage.RenameInstanceAsync(oldName, newName);
                if (success)
                {
                    WeakReferenceMessenger.Default.Send(new MinecraftInstanceModifiedMessage(newName));
                }
                else
                {
                    SetProperty(ref _instanceName, oldName, true, nameof(InstanceName));
                    _messageBoxService.Show(
                        string.Format(Lang.MessageBox_Warn_UnableRenameInstance, oldName, newName),
                        Lang.MessageBox_Warn,
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "重命名实例失败: \"{OldName}\" -> \"{NewName}\"", oldName, newName);
                SetProperty(ref _instanceName, oldName, true, nameof(InstanceName));
                _messageBoxService.Show(
                    string.Format(Lang.MessageBox_Error_RenameInstanceFailed, oldName, newName, ObjectFormatter.Format(ex)),
                    Lang.MessageBox_Error,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
