using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using iNKORE.UI.WPF.Modern.Controls;
using MCBS.WpfApp.Attributes;
using MCBS.WpfApp.Commands;
using MCBS.WpfApp.Messages;
using MCBS.WpfApp.Models;
using MCBS.WpfApp.Resources.Strings;
using MCBS.WpfApp.Services;
using MCBS.WpfApp.ViewModels.DialogBoxs;
using Microsoft.Extensions.Logging;
using QuanLib.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;

namespace MCBS.WpfApp.ViewModels.Home
{
    public partial class InstanceManageViewModel : LaunchViewModel
    {
        public InstanceManageViewModel(
            PageNavigateCommand pageNavigateCommand,
            IInstanceListStorage instanceListStorage,
            IMessageBoxAsyncService messageService,
            IDialogBoxService<CreateInstanceViewModel> createInstanceDialogService,
            ILogger<InstanceManageViewModel> logger) : base(pageNavigateCommand, instanceListStorage, logger)
        {
            ArgumentNullException.ThrowIfNull(messageService, nameof(messageService));
            ArgumentNullException.ThrowIfNull(createInstanceDialogService, nameof(createInstanceDialogService));
            ArgumentNullException.ThrowIfNull(logger, nameof(logger));

            _messageService = messageService;
            _createInstanceDialogService = createInstanceDialogService;
            _logger = logger;
        }

        private readonly IMessageBoxAsyncService _messageService;
        private readonly IDialogBoxService<CreateInstanceViewModel> _createInstanceDialogService;
        private readonly ILogger<InstanceManageViewModel> _logger;

        private MinecraftInstanceConfig.Model? _selectedInstance;

        protected override string PageToken => nameof(Pages.Home.InstanceManagePage);

        [ManualObservableProperty]
        public MinecraftInstanceConfig.Model? SelectedInstance
        {
            get => _selectedInstance;
            set
            {
                bool canChange = OnSelectedInstanceChanging(_selectedInstance, value);

                if (canChange)
                {
                    MinecraftInstanceConfig.Model? oldValue = _selectedInstance;
                    if (SetProperty(ref _selectedInstance, value))
                        OnSelectedInstanceChanged(oldValue, value);
                }
                else
                {
                    Application.Current?.Dispatcher.BeginInvoke(() =>
                        WeakReferenceMessenger.Default.Send(new UISynchronizationMessage(nameof(SelectedInstance)), nameof(InstanceManageViewModel)));

                    if (_logger.IsEnabled(LogLevel.Debug))
                        _logger.LogDebug("当前实例 \"{CurrentInstance}\" 请求切换到目标实例 \"{TargetInstance}\" 已被拦截并重置", _selectedInstance?.InstanceName, value?.InstanceName);
                }
            }
        }

        private bool OnSelectedInstanceChanging(MinecraftInstanceConfig.Model? oldValue, MinecraftInstanceConfig.Model? newValue)
        {
            string oldInstanceName = oldValue?.InstanceName ?? string.Empty;
            string newInstanceName = newValue?.InstanceName ?? string.Empty;

            if (oldInstanceName != newInstanceName)
                return WeakReferenceMessenger.Default.Send(new MinecraftInstanceSwitchRequestMessage(oldInstanceName, newInstanceName));

            return true;
        }

        private void OnSelectedInstanceChanged(MinecraftInstanceConfig.Model? oldValue, MinecraftInstanceConfig.Model? newValue)
        {
            string? oldInstanceName = oldValue?.InstanceName;
            string? newInstanceName = newValue?.InstanceName;

            if (oldInstanceName != newInstanceName)
                CurrentInstance = newValue;
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.PropertyName == nameof(CurrentInstance) && !ReferenceEquals(SelectedInstance, CurrentInstance))
                SelectedInstance = CurrentInstance;
        }

        [RelayCommand]
        public async Task CreateInstance()
        {
            CreateInstanceViewModel viewModel = await _createInstanceDialogService.ShowAsync();
            if (viewModel.DialogResult != ContentDialogResult.Primary)
                return;

            try
            {
                var model = viewModel.CreateConfigModel();
                await InstanceListStorage.AddInstanceAsync(model);
                await Reload();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建实例“{InstanceName}”失败", viewModel.InstanceName);
                await _messageService.ShowAsync(
                    string.Format(Lang.MessageBox_Error_CreateInstanceFailed, viewModel.InstanceName, ObjectFormatter.Format(ex)),
                    Lang.MessageBox_Error,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }
        }

        [RelayCommand]
        public async Task DeleteInstance(string instanceName)
        {
            if (string.IsNullOrEmpty(instanceName))
                return;

            MessageBoxResult result = await _messageService.ShowAsync(
                string.Format(Lang.MessageBox_Warn_ConfirmDeleteInstance, instanceName),
                Lang.MessageBox_Info,
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning,
                MessageBoxResult.No);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                bool success = await InstanceListStorage.RemoveInstanceAsync(instanceName);
                if (success)
                {
                    await Reload();
                }
                else
                {
                    await _messageService.ShowAsync(
                        string.Format(Lang.MessageBox_Warn_UnableDeleteInstance, instanceName),
                        Lang.MessageBox_Warn,
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除实例“{InstanceName}”失败", instanceName);
                await _messageService.ShowAsync(
                    string.Format(Lang.MessageBox_Error_DeleteInstanceFailed, instanceName, ObjectFormatter.Format(ex)),
                    Lang.MessageBox_Error,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }
        }

        [RelayCommand]
        public async Task RefreshList()
        {
            await SaveCurrentInstance();
            await Reload();
        }
    }
}
