using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Downloader;
using MCBS.WpfApp.Messages;
using MCBS.WpfApp.Resources.Strings;
using MCBS.WpfApp.Services;
using Microsoft.Extensions.Logging;
using QuanLib.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Input;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace MCBS.WpfApp.Commands
{
    public class DependencyDownloadCommand : StartDownloadCommand
    {
        public DependencyDownloadCommand(IDownloadViewModel downloadViewModel, IDownload download, ILogger? logger = null) : base(download, logger)
        {
            ArgumentNullException.ThrowIfNull(downloadViewModel, nameof(downloadViewModel));

            _downloadViewModel = downloadViewModel;
            _logger = logger;
        }

        private readonly IDownloadViewModel _downloadViewModel;
        private readonly ILogger? _logger;
        private readonly SemaphoreSlim _semaphore = new(1);

        public override bool CanExecute(object? parameter)
        {
            return _downloadViewModel.IsBusy == false && !IsRunning && base.CanExecute(parameter);
        }

        public override async Task ExecuteAsync(object? parameter)
        {
            await _semaphore.WaitAsync();

            _downloadViewModel.IsBusy = null;
            _downloadViewModel.Progress = 0.0;
            IsRunning = true;
            CanBeCanceled = true;
            IsCancellationRequested = false;

            try
            {
                if (_downloadViewModel.IsReady)
                {
                    // 下载任务已就绪，无需等待依赖项
                }
                else if (_downloadViewModel.Dependent is IDownloadViewModel dependent)
                {
                    if (dependent.IsBusy != false)
                    {
                        while (dependent.IsBusy != false)
                            await Task.Delay(100);
                    }
                    else
                    {
                        IAsyncRelayCommand startCommand = dependent.StartCommand;
                        if (!startCommand.IsRunning && startCommand.CanExecute(null))
                            await startCommand.ExecuteAsync(null);
                        if (startCommand.IsCancellationRequested)
                            return;

                        for (int i = 0; i < 10; i++)
                        {
                            if (_downloadViewModel.IsReady)
                                break;
                            else
                                await Task.Delay(100);
                        }
                    }

                    if (!_downloadViewModel.IsReady)
                    {
                        _logger?.LogWarning("依赖项加载失败，当前下载任务已自动取消");
                        return;
                    }
                }
                else
                {
                    _logger?.LogWarning("下载任务未就绪，且找不到依赖项");
                    return;
                }

                try
                {
                    await _downloadViewModel.LoadDownloadAsync();
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "加载下载任务时发生错误");
                    WeakReferenceMessenger.Default.Send(
                        new UnableDownloadMessage(_downloadViewModel,
                        string.Format(Lang.MessageBox_Error_UnableDownload, ObjectFormatter.Format(ex))),
                        _downloadViewModel.ScopeProvider.ScopeToken);
                    return; 
                }

                _downloadViewModel.IsBusy = true;
                await base.ExecuteAsync(parameter);
            }
            finally
            {
                _downloadViewModel.IsBusy = false;
                _downloadViewModel.Progress = 0.0;
                IsRunning = false;
                CanBeCanceled = false;
                NotifyCanExecuteChanged();
                _semaphore.Release();
            }
        }

        public override void Cancel()
        {
            _downloadViewModel.StartCommand.Cancel();
            base.Cancel();
        }
    }
}
