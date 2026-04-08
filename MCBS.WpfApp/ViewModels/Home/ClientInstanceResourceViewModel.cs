using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using iNKORE.UI.WPF.Modern.Common.IconKeys;
using MCBS.Common.Services;
using MCBS.Services;
using MCBS.WpfApp.Attributes;
using MCBS.WpfApp.Messages;
using MCBS.WpfApp.Models;
using MCBS.WpfApp.Resources.Strings;
using MCBS.WpfApp.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;

namespace MCBS.WpfApp.ViewModels.Home
{
    [ExcludeFromDI]
    public partial class ClientInstanceResourceViewModel : ObservableObject
    {
        public ClientInstanceResourceViewModel(
            IConfigService configService,
            IScopedMinecraftPathFactory pathFactory,
            IMessageBoxAsyncService messageBoxService,
            IAsyncHashComputeService hashComputeService)
        {
            ArgumentNullException.ThrowIfNull(configService, nameof(configService));
            ArgumentNullException.ThrowIfNull(pathFactory, nameof(pathFactory));
            ArgumentNullException.ThrowIfNull(messageBoxService, nameof(messageBoxService));
            ArgumentNullException.ThrowIfNull(hashComputeService, nameof(hashComputeService));

            _configService = configService;
            _pathFactory = pathFactory;
            _messageBoxService = messageBoxService;
            _hashComputeService = hashComputeService;

            VersionJsonFile = CreateVersionJsonFileModel(null);
            AssetManifestFile = CreateAssetManifestFileModel(null);
            ClientCoreFile = CreateClientCoreFileModel(null);
            ResourcePacks = [];
            LanguageFiles = [];
            SelectedResourcePacks = [];
            SelectedLanguageFile = null;

            GlobalFileModel emptyFileModel = new(string.Empty, 0, GlobalFileStatus.NotFound);
            GlobalVersionJsonFile = emptyFileModel;
            GlobalAssetManifestFile = emptyFileModel;
            GlobalClientCoreFile = emptyFileModel;
            GlobalLanguageFile = emptyFileModel;
        }

        private readonly IConfigService _configService;
        private readonly IScopedMinecraftPathFactory _pathFactory;
        private readonly IMessageBoxAsyncService _messageBoxService;
        private readonly IAsyncHashComputeService _hashComputeService;

        [ObservableProperty]
        public partial bool IsServer { get; set; }

        [ObservableProperty]
        public partial bool UseGlobalResources { get; set; }

        [ObservableProperty]
        public partial FileModel VersionJsonFile { get; set; }

        [ObservableProperty]
        public partial FileModel AssetManifestFile { get; set; }

        [ObservableProperty]
        public partial FileModel ClientCoreFile { get; set; }

        [ObservableProperty]
        public partial AssetFileModel? SelectedLanguageFile { get; set; }

        [ObservableProperty]
        public partial ObservableCollection<FileInfo> SelectedResourcePacks { get; set; }

        [ObservableProperty]
        public partial ObservableCollection<AssetFileModel> LanguageFiles { get; set; }

        [ObservableProperty]
        public partial ObservableCollection<FileInfo> ResourcePacks { get; set; }

        [ObservableProperty]
        public partial GlobalFileModel GlobalVersionJsonFile { get; set; }

        [ObservableProperty]
        public partial GlobalFileModel GlobalAssetManifestFile { get; set; }

        [ObservableProperty]
        public partial GlobalFileModel GlobalClientCoreFile { get; set; }

        [ObservableProperty]
        public partial GlobalFileModel GlobalLanguageFile { get; set; }

        protected override async void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            string propertyName = nameof(UseGlobalResources);
            if (e.PropertyName == propertyName)
            {
                var model = (MinecraftInstanceConfig.Model)_configService.GetCurrentConfig();
                if (UseGlobalResources != model.UseGlobalResources)
                {
                    _configService.SetPropertyValue(propertyName, UseGlobalResources);
                    await _configService.GetConfigStorage().SaveConfigAsync();
                    WeakReferenceMessenger.Default.Send(new MinecraftInstanceModifiedMessage(model.InstanceName));
                }
            }
        }

        [RelayCommand]
        private async Task Refresh()
        {
            var model = (MinecraftInstanceConfig.Model)_configService.GetCurrentConfig();
            IsServer = model.IsServer;
            UseGlobalResources = model.UseGlobalResources;

            if (IsServer)
            {
                if (!UseGlobalResources)
                {
                    UseGlobalResources = true;
                    await _messageBoxService.ShowAsync(Lang.MessageBox_ServerAutoEnabledGlobalResources, Lang.MessageBox_Warn, MessageBoxButton.OK);
                }
            }
            else
            {
                string gameFolder = Path.GetFullPath(model.MinecraftPath);
                string gameVersion = model.MinecraftVersion;
                ClientInstanceResource clientInstanceResource = new(gameFolder, gameVersion);
                await clientInstanceResource.Initialize();

                VersionJsonFile = CreateVersionJsonFileModel(CreateFileInfo(clientInstanceResource.VersionJsonFile));
                AssetManifestFile = CreateAssetManifestFileModel(CreateFileInfo(clientInstanceResource.AssetManifestFile));
                ClientCoreFile = CreateClientCoreFileModel(CreateFileInfo(clientInstanceResource.ClientCoreFile));

                string languageName = clientInstanceResource.GetSelectedLanguageName() + ".json";
                string[] resourcePackNames = clientInstanceResource.GetSelectedResourcePacks().ToArray();

                IEnumerable<AssetFileModel> languageFiles = clientInstanceResource.GetLanguageFiles().ToArray();
                IEnumerable<FileInfo> resourcePacks = clientInstanceResource.GetResourcePacks().Select(f => new FileInfo(f)).ToArray();

                SelectedLanguageFile = languageFiles.FirstOrDefault(s => s.AssetName == languageName);
                if (SelectedLanguageFile is not null)
                    languageFiles = languageFiles.Where(s => !s.Equals(SelectedLanguageFile));

                SelectedResourcePacks = new(resourcePacks
                    .Where(s => Array.IndexOf(resourcePackNames, s.Name) != -1)
                    .OrderBy(s => Array.IndexOf(resourcePackNames, s.Name)));
                if (SelectedResourcePacks.Count > 0)
                    resourcePacks = resourcePacks.Except(SelectedResourcePacks);

                LanguageFiles = new(languageFiles);
                ResourcePacks = new(resourcePacks);
            }

            if (UseGlobalResources)
            {
                IMinecraftPathProvider pathProvider = _pathFactory.CreateProvider(model.MinecraftVersion);
                GlobalInstanceResource globalInstanceResource = new(pathProvider, _hashComputeService);
                GlobalVersionJsonFile = globalInstanceResource.GetVersionJsonFile();
                GlobalAssetManifestFile = await globalInstanceResource.GetAssetManifestFileAsync();
                GlobalClientCoreFile = await globalInstanceResource.GetClientCoreFileAsync();
                GlobalLanguageFile = await globalInstanceResource.GetLanguageFileAsync(model.Language);
            }
        }

        private static FileInfo? CreateFileInfo(string? filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return null;

            return new FileInfo(filePath);
        }

        private static FileModel CreateVersionJsonFileModel(FileInfo? fileInfo)
        {
            return new FileModel(fileInfo, nameof(Lang.ClientInstanceResource_VersionJson), SegoeFluentIcons.Document);
        }

        private static FileModel CreateAssetManifestFileModel(FileInfo? fileInfo)
        {
            return new FileModel(fileInfo, nameof(Lang.ClientInstanceResource_AssetManifest), SegoeFluentIcons.Document);
        }

        private static FileModel CreateClientCoreFileModel(FileInfo? fileInfo)
        {
            return new FileModel(fileInfo, nameof(Lang.ClientInstanceResource_ClientCore), SegoeFluentIcons.Package);
        }

        public class Factory : IViewModelFactory<ClientInstanceResourceViewModel>
        {
            public Factory(
                IInstanceListStorage instanceListStorage,
                IScopedMinecraftPathFactory pathFactory,
                IMessageBoxAsyncService messageBoxService,
                IAsyncHashComputeService hashComputeService)
            {
                ArgumentNullException.ThrowIfNull(instanceListStorage, nameof(instanceListStorage));
                ArgumentNullException.ThrowIfNull(pathFactory, nameof(pathFactory));
                ArgumentNullException.ThrowIfNull(messageBoxService, nameof(messageBoxService));
                ArgumentNullException.ThrowIfNull(hashComputeService, nameof(hashComputeService));

                _instanceListStorage = instanceListStorage;
                _pathFactory = pathFactory;
                _messageBoxService = messageBoxService;
                _hashComputeService = hashComputeService;
            }

            private readonly IInstanceListStorage _instanceListStorage;
            private readonly IScopedMinecraftPathFactory _pathFactory;
            private readonly IMessageBoxAsyncService _messageBoxService;
            private readonly IAsyncHashComputeService _hashComputeService;

            public ClientInstanceResourceViewModel Create(string viewModel)
            {
                IConfigStorage configStorage = _instanceListStorage.GetInstanceStorage(viewModel);
                IConfigService configService = configStorage.GetConfig();
                return new ClientInstanceResourceViewModel(configService, _pathFactory, _messageBoxService, _hashComputeService);
            }
        }
    }
}
