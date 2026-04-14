using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Downloader;
using MCBS.Common;
using MCBS.Common.Services;
using MCBS.WpfApp.Attributes;
using MCBS.WpfApp.Commands;
using MCBS.WpfApp.Events;
using MCBS.WpfApp.Messages;
using MCBS.WpfApp.Services;
using MCBS.WpfApp.Services.Implementations;
using Microsoft.Extensions.Logging;
using QuanLib.Downloader;
using QuanLib.Downloader.Services;
using QuanLib.Minecraft.Downloading;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace MCBS.WpfApp.ViewModels.Downloading
{
    [ExcludeFromDI]
    public partial class VersionJsonDownloadViewModel : ObservableObject, IDownloadViewModel, IDisposable
    {
        public VersionJsonDownloadViewModel(
            string gameVersion,
            IScopeProvider scopeProvider,
            IDownloadViewModel versionManifestDownload,
            IDownloadCompletedHook<VersionManifest> downloadCompletedHook,
            IManifestResourceProvider manifestResourceProvider,
            IDownloadConfigurationProvider configurationProvider,
            ILoggerFactory? loggerFactory)
        {
            ArgumentException.ThrowIfNullOrEmpty(gameVersion, nameof(gameVersion));
            ArgumentNullException.ThrowIfNull(scopeProvider, nameof(scopeProvider));
            ArgumentNullException.ThrowIfNull(versionManifestDownload, nameof(versionManifestDownload));
            ArgumentNullException.ThrowIfNull(downloadCompletedHook, nameof(downloadCompletedHook));
            ArgumentNullException.ThrowIfNull(manifestResourceProvider, nameof(manifestResourceProvider));
            ArgumentNullException.ThrowIfNull(configurationProvider, nameof(configurationProvider));

            _gameVersion = gameVersion;
            _scopeProvider = scopeProvider;
            _versionManifestDownload = versionManifestDownload;
            _downloadCompletedHook = downloadCompletedHook;
            _manifestResourceProvider = manifestResourceProvider;
            _configurationProvider = configurationProvider;
            _loggerFactory = loggerFactory;

            _lazyDownload = new LazyDownload();
            IsBusy = false;
            Progress = 0.0;
            StartCommand = new DependencyDownloadCommand(this, _lazyDownload, loggerFactory?.CreateLogger<VersionJsonDownloadViewModel>());
            StopCommand = new StopDownloadCommand(_lazyDownload);
            PauseCommand = new PauseDownloadCommand(_lazyDownload);
            ResumeCommand = new ResumeDownloadCommand(_lazyDownload);
        }

        private readonly string _gameVersion;
        private readonly IScopeProvider _scopeProvider;
        private readonly IDownloadViewModel _versionManifestDownload;
        private readonly IDownloadCompletedHook<VersionManifest> _downloadCompletedHook;
        private readonly IManifestResourceProvider _manifestResourceProvider;
        private readonly IDownloadConfigurationProvider _configurationProvider;
        private readonly ILoggerFactory? _loggerFactory;
        private readonly LazyDownload _lazyDownload;
        protected readonly SemaphoreSlim _semaphore = new(1);
        private EnhancedDownload? _download;
        private Progress<DownloadProgress>? _progress;

        public event EventHandler<FileDownloadStartedEventArgs>? DownloadStarted;

        public event EventHandler<FileDownloadCompletedEventArgs>? DownloadCompleted;

        public string GameVersion => _gameVersion;

        public IScopeProvider ScopeProvider => _scopeProvider;

        public IDownloadViewModel Dependent => _versionManifestDownload;

        public IDownload? DownloadTask => _download;

        public bool IsReady => _download is not null || _downloadCompletedHook.IsReady;

        [ObservableProperty]
        public partial bool? IsBusy { get; set; }

        [ObservableProperty]
        public partial double Progress { get; set; }

        public IAsyncRelayCommand StartCommand { get; }

        public IRelayCommand StopCommand { get; }

        public IRelayCommand PauseCommand { get; }

        public IRelayCommand ResumeCommand { get; }

        public async Task<IDownload> LoadDownloadAsync()
        {
            await _semaphore.WaitAsync();
            try
            {
                if (_download is not null)
                    return _download;

                VersionManifest versionManifest = await _downloadCompletedHook.LoadContentAsync();
                DownloadAsset downloadAsset = _manifestResourceProvider.GetDownloadAsset(versionManifest, _gameVersion);

                _progress = new Progress<DownloadProgress>(OnProgressChanged);
                _download = EnhancedDownloadBuilder.New()
                    .WithConfiguration(_configurationProvider.GlobalConfiguration)
                    .WithUrl(downloadAsset.Url)
                    .WithFileLocation(downloadAsset.FilePath)
                    .WithProgress(_progress)
                    .WithMinProgressInterval(50)
                    .WithLogging(_loggerFactory)
                    //.OpenFileStreamAfterCompletion()  // 由于下载完成事件时序问题，暂不启用该选项。但有 ILocalFileHook 兜底，因此不影响后续使用。
                    .Build();

                _download.DownloadStarted += Download_DownloadStarted;
                _download.DownloadFileCompleted += Download_DownloadFileCompleted;

                _lazyDownload.SetOwner(_download);
                return _download;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private void OnProgressChanged(DownloadProgress progress)
        {
            Progress = progress.TotalFileSize == 0 ? 0 : progress.DownloadedFileSize / (double)progress.TotalFileSize;
        }

        private void Download_DownloadStarted(object? sender, DownloadStartedEventArgs e)
        {
            if (_download is not null)
            {
                FileDownloadStartedEventArgs eventArgs = new(_download.Url, string.Empty, e.TotalBytesToReceive);
                DownloadStarted?.Invoke(this, eventArgs);
                WeakReferenceMessenger.Default.Send(new DownloadStartedMessage(this, eventArgs), _scopeProvider.ScopeToken);
            }
        }

        // TODO: 当下载的目标是文件流时，即使启用 OpenFileStreamAfterCompletion 选项，下载完成后 ResultStream 仍然为 null。
        // TODO: 这是因为文件流的打开时机是在下载完成事件触发之后。需要调整事件触发的时机，确保在下载完成事件触发时文件流已经打开并可用。
        private void Download_DownloadFileCompleted(object? sender, AsyncCompletedEventArgs e)
        {
            if (_download is not null)
            {
                FileDownloadCompletedEventArgs eventArgs = new(_download.Status, _download.ResultStream, _download.Error);
                DownloadCompleted?.Invoke(this, eventArgs);
                WeakReferenceMessenger.Default.Send(new DownloadCompletedMessage(this, eventArgs), _scopeProvider.ScopeToken);
            }
        }

        public void Dispose()
        {
            if (_download is not null)
            {
                _download.DownloadStarted -= Download_DownloadStarted;
                _download.DownloadFileCompleted -= Download_DownloadFileCompleted;
                _download.Dispose();
            }

            _downloadCompletedHook.Dispose();
            _lazyDownload.Dispose();
            _semaphore.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
