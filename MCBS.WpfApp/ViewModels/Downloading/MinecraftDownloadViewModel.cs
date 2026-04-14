using CommunityToolkit.Mvvm.ComponentModel;
using MCBS.Common.Services;
using MCBS.Common.Services.Implementations;
using MCBS.Services;
using MCBS.WpfApp.Attributes;
using MCBS.WpfApp.Services;
using MCBS.WpfApp.Services.Implementations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuanLib.Downloader.Services;
using QuanLib.Minecraft.Downloading;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace MCBS.WpfApp.ViewModels.Downloading
{
    [ExcludeFromDI]
    public partial class MinecraftDownloadViewModel : ObservableObject, IDisposable
    {
        private IDownloadViewModel[]? _viewModels;

        public required IScopeProvider ScopeProvider { get; init; }

        public required string DownloadSource { get; init; }

        public bool IsDownloading => _viewModels is not null && Array.Exists(_viewModels, vm => vm.IsBusy != false);

        [ObservableProperty]
        public required partial VersionManifestDownloadViewModel VersionManifestDownload { get; set; }

        [ObservableProperty]
        public required partial VersionJsonDownloadViewModel VersionJsonDownload { get; set; }

        [ObservableProperty]
        public required partial VersionResourceDownloadViewModel AssetManifestDownload { get; set; }

        [ObservableProperty]
        public required partial VersionResourceDownloadViewModel ClientCoreDownload { get; set; }

        [ObservableProperty]
        public required partial AssetResourceDownloadViewModel LanguageFileDownload { get; set; }

        public void Dispose()
        {
            VersionManifestDownload.Dispose();
            VersionJsonDownload.Dispose();
            AssetManifestDownload.Dispose();
            ClientCoreDownload.Dispose();
            LanguageFileDownload.Dispose();
            GC.SuppressFinalize(this);
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.PropertyName is
                nameof(VersionManifestDownload) or
                nameof(VersionJsonDownload) or
                nameof(AssetManifestDownload) or
                nameof(ClientCoreDownload) or
                nameof(LanguageFileDownload))
            {
                _viewModels =
                [
                    VersionManifestDownload,
                    VersionJsonDownload,
                    AssetManifestDownload,
                    ClientCoreDownload,
                    LanguageFileDownload
                ];
            }
        }

        public class Factory : IMinecraftDownloadViewModelFactory
        {
            public Factory(IServiceProvider serviceProvider)
            {
                ArgumentNullException.ThrowIfNull(serviceProvider, nameof(serviceProvider));

                _serviceProvider = serviceProvider;
            }

            private readonly IServiceProvider _serviceProvider;

            public MinecraftDownloadViewModel Create(string gameVersion, string language, string downloadSource)
            {
                var pathFactory = _serviceProvider.GetRequiredService<IScopedMinecraftPathFactory>();
                var pathProvider = pathFactory.CreateProvider(gameVersion);
                var downloadProvider =
                    _serviceProvider.GetKeyedService<IMinecraftDownloadProvider>(downloadSource) ??
                    _serviceProvider.GetRequiredService<IMinecraftDownloadProvider>();
                var configurationProvider = _serviceProvider.GetRequiredService<IDownloadConfigurationProvider>();
                var assetMatchService = _serviceProvider.GetRequiredService<ILanguageAssetMatchService>();
                var loggerFactory = _serviceProvider.GetRequiredService<ILoggerFactory>();
                var scopeProvider = _serviceProvider.GetRequiredService<IScopeProvider>();

                ManifestResourceProvider manifestResourceProvider = new(pathProvider, downloadProvider);
                IndexFileResourceProvider indexFileResourceProvider = new(pathProvider, downloadProvider);
                ClientCoreResourceProvider clientCoreResourceProvider = new(pathProvider, downloadProvider);
                LanguageResourceProvider languageResourceProvider = new(pathProvider, downloadProvider, assetMatchService);

                VersionManifestDownloadViewModel versionManifestDownload = new(scopeProvider, downloadProvider, configurationProvider, loggerFactory);
                RemoteVersionManifestHook remoteVersionManifestHook = new(loggerFactory.CreateLogger<RemoteVersionManifestHook>());
                remoteVersionManifestHook.Binding(versionManifestDownload);

                VersionJsonDownloadViewModel versionJsonDownload = new(gameVersion, scopeProvider, versionManifestDownload, remoteVersionManifestHook, manifestResourceProvider, configurationProvider, loggerFactory);
                LocalVersionJsonHook localVersionJsonHook = new(pathProvider);
                RemoteVersionJsonHook remoteVersionJsonHook = new(loggerFactory.CreateLogger<RemoteVersionJsonHook>());
                remoteVersionJsonHook.Binding(versionJsonDownload);

                VersionResourceDownloadViewModel assetManifestDownload = new(scopeProvider, versionJsonDownload, remoteVersionJsonHook, localVersionJsonHook, indexFileResourceProvider, configurationProvider, loggerFactory);
                LocalAssetManifestHook localAssetManifestHook = new(pathProvider);
                RemoteAssetManifestHook remoteAssetManifestHook = new(loggerFactory.CreateLogger<RemoteAssetManifestHook>());
                remoteAssetManifestHook.Binding(assetManifestDownload);

                VersionResourceDownloadViewModel clientCoreDownload = new(scopeProvider, versionJsonDownload, remoteVersionJsonHook, localVersionJsonHook, clientCoreResourceProvider, configurationProvider, loggerFactory);
                AssetResourceDownloadViewModel languageFileDownload = new(language, scopeProvider, assetManifestDownload, remoteAssetManifestHook, localAssetManifestHook, languageResourceProvider, configurationProvider, loggerFactory);

                return new MinecraftDownloadViewModel
                {
                    ScopeProvider = scopeProvider,
                    DownloadSource = downloadSource,
                    VersionManifestDownload = versionManifestDownload,
                    VersionJsonDownload = versionJsonDownload,
                    AssetManifestDownload = assetManifestDownload,
                    ClientCoreDownload = clientCoreDownload,
                    LanguageFileDownload = languageFileDownload
                };
            }
        }
    }
}
