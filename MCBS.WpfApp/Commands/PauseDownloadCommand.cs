using CommunityToolkit.Mvvm.Input;
using Downloader;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.WpfApp.Commands
{
    public class PauseDownloadCommand : IRelayCommand
    {
        public PauseDownloadCommand(IDownload download)
        {
            ArgumentNullException.ThrowIfNull(download, nameof(download));

            _download = download;
        }

        private readonly IDownload _download;

        public event EventHandler? CanExecuteChanged;

        public virtual bool CanExecute(object? parameter)
        {
            return _download.Status == DownloadStatus.Running;
        }

        public virtual void Execute(object? parameter)
        {
            _download.Pause();
            NotifyCanExecuteChanged();
        }

        public virtual void NotifyCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
