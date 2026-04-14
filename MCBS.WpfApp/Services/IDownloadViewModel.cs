using CommunityToolkit.Mvvm.Input;
using Downloader;
using MCBS.WpfApp.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace MCBS.WpfApp.Services
{
    public interface IDownloadViewModel
    {
        public event EventHandler<FileDownloadStartedEventArgs>? DownloadStarted;

        public event EventHandler<FileDownloadCompletedEventArgs>? DownloadCompleted;

        public IScopeProvider ScopeProvider { get; }

        public IDownloadViewModel? Dependent { get; }

        public IDownload? DownloadTask { get; }

        public bool IsReady { get; }

        public bool? IsBusy { get; set; }

        public double Progress { get; set; }

        public IAsyncRelayCommand StartCommand { get; }

        public IRelayCommand StopCommand { get; }

        public IRelayCommand PauseCommand { get; }

        public IRelayCommand ResumeCommand { get; }

        public Task<IDownload> LoadDownloadAsync();
    }
}
