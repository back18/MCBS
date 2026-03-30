using iNKORE.UI.WPF.Modern.Controls;
using MCBS.WpfApp.ViewModels.DialogBoxs;
using MCBS.WpfApp.Views.DialogBoxs;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.WpfApp.Services.Implementations
{
    public class CreateInstanceDialogService : IDialogBoxService<CreateInstanceViewModel>
    {
        public CreateInstanceDialogService(
            IInstanceListStorage instanceListStorage,
            IMessageBoxService messageBoxService,
            IFolderDialogService folderDialogService)
        {
            ArgumentNullException.ThrowIfNull(instanceListStorage, nameof(instanceListStorage));
            ArgumentNullException.ThrowIfNull(messageBoxService, nameof(messageBoxService));
            ArgumentNullException.ThrowIfNull(folderDialogService, nameof(folderDialogService));

            _instanceListStorage = instanceListStorage;
            _messageBoxService = messageBoxService;
            _folderDialogService = folderDialogService;
        }

        private readonly IInstanceListStorage _instanceListStorage;
        private readonly IMessageBoxService _messageBoxService;
        private readonly IFolderDialogService _folderDialogService;

        public async Task<CreateInstanceViewModel> ShowAsync()
        {
            CreateInstanceViewModel viewModel = new(_instanceListStorage, _messageBoxService, _folderDialogService);
            CreateInstanceDialog dialog = new(viewModel);
            ContentDialogResult dialogResult = await dialog.ShowAsync();
            viewModel.DialogResult = dialogResult;
            return viewModel;
        }
    }
}
