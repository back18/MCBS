using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Downloader;
using MCBS.WpfApp.Attributes;
using MCBS.WpfApp.Commands;
using MCBS.WpfApp.Events;
using MCBS.WpfApp.Messages;
using MCBS.WpfApp.Services;
using Microsoft.Extensions.Logging;
using QuanLib.Downloader;
using QuanLib.Downloader.Services;
using QuanLib.Minecraft.Downloading;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows.Input;

namespace MCBS.WpfApp.ViewModels.Downloading
{
    [ExcludeFromDI]
    public partial class VersionManifestDownloadViewModel : ObservableObject, IDownloadViewModel, IDisposable
    {
        public VersionManifestDownloadViewModel(
            IScopeProvider scopeProvider,
            IMinecraftDownloadProvider downloadProvider,
            IDownloadConfigurationProvider configurationProvider,
            ILoggerFactory? loggerFactory)
        {
            ArgumentNullException.ThrowIfNull(scopeProvider, nameof(scopeProvider));
            ArgumentNullException.ThrowIfNull(downloadProvider, nameof(downloadProvider));
            ArgumentNullException.ThrowIfNull(configurationProvider, nameof(configurationProvider));

            _scopeProvider = scopeProvider;
            _progress = new Progress<DownloadProgress>(OnProgressChanged);
            _download = EnhancedDownloadBuilder.New()
                .WithConfiguration(configurationProvider.GlobalConfiguration)
                .WithUrl(downloadProvider.VersionManifestUrl)
                .WithProgress(_progress)
                .WithMinProgressInterval(50)
                .WithLogging(loggerFactory)
                .Build();

            _download.DownloadStarted += Download_DownloadStarted;
            _download.DownloadFileCompleted += Download_DownloadFileCompleted;

            IsBusy = false;
            Progress = 0.0;
            StartCommand = new StartDownloadCommand(_download, loggerFactory?.CreateLogger<VersionManifestDownloadViewModel>());
            StopCommand = new StopDownloadCommand(_download);
            PauseCommand = new PauseDownloadCommand(_download);
            ResumeCommand = new ResumeDownloadCommand(_download);
        }

        private readonly IScopeProvider _scopeProvider;
        private readonly EnhancedDownload _download;
        private readonly Progress<DownloadProgress> _progress;

        public event EventHandler<FileDownloadStartedEventArgs>? DownloadStarted;

        public event EventHandler<FileDownloadCompletedEventArgs>? DownloadCompleted;

        public IScopeProvider ScopeProvider => _scopeProvider;

        public IDownloadViewModel? Dependent => null;

        public IDownload? DownloadTask => _download;

        public bool IsReady => true;

        [ObservableProperty]
        public partial bool? IsBusy { get; set; }

        [ObservableProperty]
        public partial double Progress { get; set; }

        public IAsyncRelayCommand StartCommand { get; }

        public IRelayCommand StopCommand { get; }

        public IRelayCommand PauseCommand { get; }

        public IRelayCommand ResumeCommand { get; }

        public Task<IDownload> LoadDownloadAsync()
        {
            return Task.FromResult<IDownload>(_download);
        }

        private void OnProgressChanged(DownloadProgress progress)
        {
            Progress = progress.TotalFileSize == 0 ? 0 : progress.DownloadedFileSize / (double)progress.TotalFileSize;
        }

        private void Download_DownloadStarted(object? sender, DownloadStartedEventArgs e)
        {
            FileDownloadStartedEventArgs eventArgs = new(_download.Url, string.Empty, e.TotalBytesToReceive);
            DownloadStarted?.Invoke(this, eventArgs);
            WeakReferenceMessenger.Default.Send(new DownloadStartedMessage(this, eventArgs), _scopeProvider.ScopeToken);
        }

        private void Download_DownloadFileCompleted(object? sender, AsyncCompletedEventArgs e)
        {
            FileDownloadCompletedEventArgs eventArgs = new(_download.Status, _download.ResultStream, _download.Error);
            DownloadCompleted?.Invoke(this, eventArgs);
            WeakReferenceMessenger.Default.Send(new DownloadCompletedMessage(this, eventArgs), _scopeProvider.ScopeToken);
        }

        public void Dispose()
        {
            _download.DownloadStarted -= Download_DownloadStarted;
            _download.DownloadFileCompleted -= Download_DownloadFileCompleted;
            _download.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
