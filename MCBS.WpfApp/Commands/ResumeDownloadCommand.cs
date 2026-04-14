using CommunityToolkit.Mvvm.Input;
using Downloader;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.WpfApp.Commands
{
    public class ResumeDownloadCommand : IRelayCommand
    {
        public ResumeDownloadCommand(IDownload download)
        {
            ArgumentNullException.ThrowIfNull(download, nameof(download));

            _download = download;
        }

        private readonly IDownload _download;

        public event EventHandler? CanExecuteChanged;

        public virtual bool CanExecute(object? parameter)
        {
            return _download.Status == DownloadStatus.Paused;
        }

        public virtual void Execute(object? parameter)
        {
            _download.Resume();
            NotifyCanExecuteChanged();
        }

        public virtual void NotifyCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
