using CommunityToolkit.Mvvm.Input;
using iNKORE.UI.WPF.Modern.Controls;
using MCBS.WpfApp.Commands;
using MCBS.WpfApp.Resources.Strings;
using MCBS.WpfApp.Services;
using MCBS.WpfApp.ViewModels.DialogBoxs;
using Microsoft.Extensions.Logging;
using QuanLib.Core;
using System;
using System.Collections.Generic;
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

        protected override string PageToken => nameof(Pages.Home.InstanceManagePage);

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
