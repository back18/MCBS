using Downloader;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;

namespace MCBS.WpfApp.Services.Implementations
{
    public class LazyDownload : ILazyDownload
    {
        private IDownload? _owner;
        private EventHandler<DownloadStartedEventArgs>? DownloadStartedDelegate;
        private EventHandler<AsyncCompletedEventArgs>? DownloadFileCompletedDelegate;
        private EventHandler<DownloadProgressChangedEventArgs>? DownloadProgressChangedDelegate;
        private EventHandler<DownloadProgressChangedEventArgs>? ChunkDownloadProgressChangedDelegate;

        public bool HasOwner => _owner is not null;

        public string Url => _owner?.Url ?? string.Empty;

        public string Folder => _owner?.Folder ?? string.Empty;

        public string Filename => _owner?.Filename ?? string.Empty;

        public long TotalFileSize => _owner?.TotalFileSize ?? 0;

        public long DownloadedFileSize => _owner?.DownloadedFileSize ?? 0;

        public DownloadStatus Status => _owner?.Status ?? DownloadStatus.None;

        public DownloadPackage Package
        {
            get
            {
                ThrowIfOwnerNotSet();
                return _owner.Package;
            }
        }

        public event EventHandler<DownloadStartedEventArgs> DownloadStarted
        {
            add
            {
                if (_owner is not null)
                    _owner.DownloadStarted += value;
                else
                    DownloadStartedDelegate += value;
            }
            remove
            {
                if (_owner is not null)
                    _owner.DownloadStarted -= value;
                else
                    DownloadStartedDelegate -= value;
            }
        }

        public event EventHandler<AsyncCompletedEventArgs> DownloadFileCompleted
        {
            add
            {
                if (_owner is not null)
                    _owner.DownloadFileCompleted += value;
                else
                    DownloadFileCompletedDelegate += value;
            }
            remove
            {
                if (_owner is not null)
                    _owner.DownloadFileCompleted -= value;
                else
                    DownloadFileCompletedDelegate -= value;
            }
        }

        public event EventHandler<DownloadProgressChangedEventArgs> DownloadProgressChanged
        {
            add
            {
                if (_owner is not null)
                    _owner.DownloadProgressChanged += value;
                else
                    DownloadProgressChangedDelegate += value;
            }
            remove
            {
                if (_owner is not null)
                    _owner.DownloadProgressChanged -= value;
                else
                    DownloadProgressChangedDelegate -= value;
            }
        }

        public event EventHandler<DownloadProgressChangedEventArgs> ChunkDownloadProgressChanged
        {
            add
            {
                if (_owner is not null)
                    _owner.ChunkDownloadProgressChanged += value;
                else
                    ChunkDownloadProgressChangedDelegate += value;
            }
            remove
            {
                if (_owner is not null)
                    _owner.ChunkDownloadProgressChanged -= value;
                else
                    ChunkDownloadProgressChangedDelegate -= value;
            }
        }

        public void SetOwner(IDownload owner)
        {
            ArgumentNullException.ThrowIfNull(owner, nameof(owner));
            if (_owner is not null)
                throw new InvalidOperationException("Owner has already been set.");

            _owner = owner;
            if (DownloadStartedDelegate is not null)
                _owner.DownloadStarted += DownloadStartedDelegate;
            if (DownloadFileCompletedDelegate is not null)
                _owner.DownloadFileCompleted += DownloadFileCompletedDelegate;
            if (DownloadProgressChangedDelegate is not null)
                _owner.DownloadProgressChanged += DownloadProgressChangedDelegate;
            if (ChunkDownloadProgressChangedDelegate is not null)
                _owner.ChunkDownloadProgressChanged += ChunkDownloadProgressChangedDelegate;
        }

        public Task<Stream> StartAsync(CancellationToken cancellationToken = default)
        {
            ThrowIfOwnerNotSet();
            return _owner.StartAsync(cancellationToken);
        }

        public void Stop()
        {
            ThrowIfOwnerNotSet();
            _owner.Stop();
        }

        public void Pause()
        {
            ThrowIfOwnerNotSet();
            _owner.Pause();
        }

        public void Resume()
        {
            ThrowIfOwnerNotSet();
            _owner.Resume();
        }

        [MemberNotNull(nameof(_owner))]
        private void ThrowIfOwnerNotSet()
        {
            if (_owner is null)
                throw new InvalidOperationException("Owner has not been set.");
        }

        public void Dispose()
        {
            _owner?.Dispose();
            GC.SuppressFinalize(this);
        }

        public ValueTask DisposeAsync()
        {
            try
            {
                if (_owner is not null)
                    return _owner.DisposeAsync();
                else
                    return ValueTask.CompletedTask;
            }
            finally
            {
                GC.SuppressFinalize(this);
            }
        }
    }
}
