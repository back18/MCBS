using CommunityToolkit.Mvvm.Input;
using Downloader;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.WpfApp.Commands
{
    public class StopDownloadCommand : IRelayCommand
    {
        public StopDownloadCommand(IDownload download)
        {
            ArgumentNullException.ThrowIfNull(download, nameof(download));

            _download = download;
        }

        private readonly IDownload _download;

        public event EventHandler? CanExecuteChanged;

        public virtual bool CanExecute(object? parameter)
        {
            return _download.Status is DownloadStatus.Running or DownloadStatus.Paused;
        }

        public virtual void Execute(object? parameter)
        {
            _download.Stop();
            NotifyCanExecuteChanged();
        }

        public virtual void NotifyCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
