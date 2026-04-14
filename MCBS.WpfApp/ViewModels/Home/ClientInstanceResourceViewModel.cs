using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Downloader;
using iNKORE.UI.WPF.Modern.Common.IconKeys;
using MCBS.Services;
using MCBS.WpfApp.Attributes;
using MCBS.WpfApp.Messages;
using MCBS.WpfApp.Models;
using MCBS.WpfApp.Resources.Strings;
using MCBS.WpfApp.Services;
using MCBS.WpfApp.ViewModels.Downloading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuanLib.Core;
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
    public partial class ClientInstanceResourceViewModel :
        ObservableObject,
        IRecipient<DownloadCompletedMessage>,
        IRecipient<UnableDownloadMessage>
    {
        public ClientInstanceResourceViewModel(
            IConfigService configService,
            IScopedMinecraftPathFactory pathFactory,
            IGlobalInstanceResourceProvider globalResourceProvider,
            IMinecraftDownloadViewModelFactory downloadViewModelFactory,
            IMessageBoxAsyncService messageBoxService,
            ILogger<ClientInstanceResourceViewModel> logger)
        {
            ArgumentNullException.ThrowIfNull(configService, nameof(configService));
            ArgumentNullException.ThrowIfNull(pathFactory, nameof(pathFactory));
            ArgumentNullException.ThrowIfNull(globalResourceProvider, nameof(globalResourceProvider));
            ArgumentNullException.ThrowIfNull(downloadViewModelFactory, nameof(downloadViewModelFactory));
            ArgumentNullException.ThrowIfNull(messageBoxService, nameof(messageBoxService));
            ArgumentNullException.ThrowIfNull(logger, nameof(logger));

            _configService = configService;
            _pathFactory = pathFactory;
            _globalResourceProvider = globalResourceProvider;
            _downloadViewModelFactory = downloadViewModelFactory;
            _messageBoxService = messageBoxService;
            _logger = logger;

            VersionJsonFile = CreateVersionJsonFileModel(null);
            AssetManifestFile = CreateAssetManifestFileModel(null);
            ClientCoreFile = CreateClientCoreFileModel(null);
            ResourcePacks = [];
            LanguageFiles = [];
            SelectedResourcePacks = [];
            SelectedLanguageFile = null;

            GlobalVersionJsonFile = new(string.Empty, 0, GlobalFileStatus.NotFound, SegoeFluentIcons.Document);
            GlobalAssetManifestFile = new(string.Empty, 0, GlobalFileStatus.NotFound, SegoeFluentIcons.Document);
            GlobalClientCoreFile = new(string.Empty, 0, GlobalFileStatus.NotFound, SegoeFluentIcons.Package);
            GlobalLanguageFile = new(string.Empty, 0, GlobalFileStatus.NotFound, SegoeFluentIcons.LocaleLanguage);
        }

        private readonly IConfigService _configService;
        private readonly IScopedMinecraftPathFactory _pathFactory;
        private readonly IGlobalInstanceResourceProvider _globalResourceProvider;
        private readonly IMinecraftDownloadViewModelFactory _downloadViewModelFactory;
        private readonly IMessageBoxAsyncService _messageBoxService;
        private readonly ILogger<ClientInstanceResourceViewModel> _logger;

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

        [ObservableProperty]
        public partial MinecraftDownloadViewModel? Download { get; set; }

        async void IRecipient<DownloadCompletedMessage>.Receive(DownloadCompletedMessage message)
        {
            IAsyncRelayCommand refreshCommand = RefreshGlobalResourcesCommand;
            if (!refreshCommand.IsRunning && refreshCommand.CanExecute(null))
                await refreshCommand.ExecuteAsync(null);

            if (message.EventArgs.Status is DownloadStatus.None or DownloadStatus.Failed)
            {
                Application.Current?.Dispatcher.BeginInvoke(async () =>
                {
                    string? fileName = message.Owner.DownloadTask?.Filename;
                    if (string.IsNullOrEmpty(fileName))
                        fileName = message.Owner == Download?.VersionManifestDownload ? "version_manifest.json" : "Undefined";

                    MessageBoxResult result = await _messageBoxService.ShowAsync(
                        string.Format(Lang.MessageBox_Error_DownloadFailed,
                        fileName,
                        ObjectFormatter.Format(message.EventArgs.Error)),
                        Lang.MessageBox_Error,
                        MessageBoxButton.OKCancel);

                    if (result == MessageBoxResult.OK)
                    {
                        IDownloadViewModel viewModel = message.Owner;
                        IAsyncRelayCommand downloadCommand = viewModel.StartCommand;
                        if (!downloadCommand.IsRunning && downloadCommand.CanExecute(null))
                            _ = downloadCommand.ExecuteAsync(null);
                    }
                });
            }
        }

        void IRecipient<UnableDownloadMessage>.Receive(UnableDownloadMessage message)
        {
            Application.Current?.Dispatcher.BeginInvoke(() =>
                _ = _messageBoxService.ShowAsync(message.ErrorMessage, Lang.MessageBox_Error, MessageBoxButton.OK));
        }

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

                string languageName = clientInstanceResource.GetSelectedLanguageName();
                string[] resourcePackNames = clientInstanceResource.GetSelectedResourcePacks().ToArray();

                IEnumerable<AssetFileModel> languageFiles = clientInstanceResource.GetLanguageFiles().ToArray();
                IEnumerable<FileInfo> resourcePacks = clientInstanceResource.GetResourcePacks().Select(f => new FileInfo(f)).ToArray();

                SelectedLanguageFile = languageFiles.FirstOrDefault(s => Path.GetFileNameWithoutExtension(s.AssetName) == languageName);
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

            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("实例资源 \"{InstanceName}\" 重载成功", model.InstanceName);

            if (UseGlobalResources)
            {
                IAsyncRelayCommand command = RefreshGlobalResourcesCommand;
                if (!command.IsRunning && command.CanExecute(null))
                    await command.ExecuteAsync(null);
            }
        }

        [RelayCommand]
        private async Task RefreshGlobalResources()
        {
            var model = (MinecraftInstanceConfig.Model)_configService.GetCurrentConfig();
            IMinecraftPathProvider pathProvider = _pathFactory.CreateProvider(model.MinecraftVersion);
            _globalResourceProvider.SetPathProvider(pathProvider);

            GlobalVersionJsonFile = WithIcon(_globalResourceProvider.GetVersionJsonFile(), SegoeFluentIcons.Document);
            GlobalAssetManifestFile = WithIcon(await _globalResourceProvider.GetAssetManifestFileAsync(true), SegoeFluentIcons.Document);
            GlobalClientCoreFile = WithIcon(await _globalResourceProvider.GetClientCoreFileAsync(false), SegoeFluentIcons.Package);
            GlobalLanguageFile = WithIcon(await _globalResourceProvider.GetLanguageFileAsync(model.Language, true), SegoeFluentIcons.LocaleLanguage);

            if (Download is null)
            {
                Download = _downloadViewModelFactory.Create(model.MinecraftVersion, model.Language, model.DownloadSource);
                WeakReferenceMessenger.Default.Register<DownloadCompletedMessage, string>(this, Download.ScopeProvider.ScopeToken);
                WeakReferenceMessenger.Default.Register<UnableDownloadMessage, string>(this, Download.ScopeProvider.ScopeToken);
            }
            else if (!Download.IsDownloading &&
                     (Download.DownloadSource != model.DownloadSource ||
                     Download.VersionJsonDownload.GameVersion != model.MinecraftVersion ||
                     Download.LanguageFileDownload.AssetName != model.Language))
            {
                WeakReferenceMessenger.Default.Unregister<DownloadCompletedMessage, string>(this, Download.ScopeProvider.ScopeToken);
                WeakReferenceMessenger.Default.Unregister<UnableDownloadMessage, string>(this, Download.ScopeProvider.ScopeToken);

                Download.Dispose();
                Download = _downloadViewModelFactory.Create(model.MinecraftVersion, model.Language, model.DownloadSource);

                WeakReferenceMessenger.Default.Register<DownloadCompletedMessage, string>(this, Download.ScopeProvider.ScopeToken);
                WeakReferenceMessenger.Default.Register<UnableDownloadMessage, string>(this, Download.ScopeProvider.ScopeToken);
            }

            if (_logger.IsEnabled(LogLevel.Information))
                _logger.LogInformation("实例 \"{InstanceName}\" 的全局资源重载成功，版本: {MinecraftVersion} 语言: {Language} 下载源: {DownloadSource}",
                    model.InstanceName, model.MinecraftVersion, model.Language, model.DownloadSource); 
        }

        private static GlobalFileModel WithIcon(GlobalFileModel fileModel, FontIconData iconData)
        {
            return new GlobalFileModel(fileModel.FileName, fileModel.Size, fileModel.Status, iconData);
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
            public Factory(IServiceProvider serviceProvider)
            {
                ArgumentNullException.ThrowIfNull(serviceProvider, nameof(serviceProvider));

                _serviceProvider = serviceProvider;
            }

            private readonly IServiceProvider _serviceProvider;

            public ClientInstanceResourceViewModel Create(string viewModel)
            {
                var pathFactory = _serviceProvider.GetRequiredService<IScopedMinecraftPathFactory>();
                var globalResourceProvider = _serviceProvider.GetRequiredService<IGlobalInstanceResourceProvider>();
                var downloadViewModelFactory = _serviceProvider.GetRequiredService<IMinecraftDownloadViewModelFactory>();
                var messageBoxService = _serviceProvider.GetRequiredService<IMessageBoxAsyncService>();
                var logger = _serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<ClientInstanceResourceViewModel>();
                var instanceListStorage = _serviceProvider.GetRequiredService<IInstanceListStorage>();
                IConfigStorage configStorage = instanceListStorage.GetInstanceStorage(viewModel);
                IConfigService configService = configStorage.GetConfig();

                return new ClientInstanceResourceViewModel(
                    configService,
                    pathFactory,
                    globalResourceProvider,
                    downloadViewModelFactory,
                    messageBoxService,
                    logger);
            }
        }
    }
}
