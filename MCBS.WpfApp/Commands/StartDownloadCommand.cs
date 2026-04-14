using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Downloader;
using Microsoft.Extensions.Logging;
using QuanLib.Downloader;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace MCBS.WpfApp.Commands
{
    public partial class StartDownloadCommand : ObservableObject, IAsyncRelayCommand
    {
        public StartDownloadCommand(IDownload download, ILogger? logger = null)
        {
            ArgumentNullException.ThrowIfNull(download, nameof(download));

            _download = download;
            _logger = logger;
        }

        private readonly IDownload _download;
        private readonly ILogger? _logger;
        private CancellationTokenSource? _cancellationTokenSource;

        public event EventHandler? CanExecuteChanged;

        [ObservableProperty]
        public partial Task? ExecutionTask { get; set; }

        [ObservableProperty]
        public partial bool CanBeCanceled { get; set; }

        [ObservableProperty]
        public partial bool IsCancellationRequested { get; set; }

        [ObservableProperty]
        public partial bool IsRunning { get; set; }

        public virtual bool CanExecute(object? parameter)
        {
            return !IsRunning &&
                _download.Status is
                DownloadStatus.None or
                DownloadStatus.Created or
                DownloadStatus.Completed or
                DownloadStatus.Stopped or
                DownloadStatus.Failed;
        }

        public virtual async void Execute(object? parameter)
        {
            await ExecuteAsync(parameter);
        }

        public virtual async Task ExecuteAsync(object? parameter)
        {
            _cancellationTokenSource = new();
            IsRunning = true;
            CanBeCanceled = true;
            IsCancellationRequested = false;

            try
            {
                if (_logger?.IsEnabled(LogLevel.Information) == true)
                    _logger.LogInformation("开始下载: {Url}", _download.Url);

                ExecutionTask = _download.StartAsync(_cancellationTokenSource.Token);
                await Task.Yield();
                NotifyCanExecuteChanged();
                await ExecutionTask;

                if (_download.Status == DownloadStatus.Completed && _logger?.IsEnabled(LogLevel.Information) == true)
                {
                    _logger.LogInformation("下载完成: {Url}", _download.Url);
                    string fileLocation = _download.FileLocation;
                    if (!string.IsNullOrEmpty(fileLocation))
                        _logger.LogInformation("文件已保存到: {FileLocation}", fileLocation);
                }
            }
            catch (OperationCanceledException)
            {
                IsCancellationRequested = true;
            }
            finally
            {
                _cancellationTokenSource = null;
                IsRunning = false;
                CanBeCanceled = false;
                NotifyCanExecuteChanged();
            }
        }

        public virtual void Cancel()
        {
            IsCancellationRequested = true;
            _cancellationTokenSource?.Cancel();
        }

        public virtual void NotifyCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
