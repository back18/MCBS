using Downloader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MCBS.WpfApp.Events
{
    public class FileDownloadCompletedEventArgs(DownloadStatus status, Stream? resultStream, Exception? error) : EventArgs
    {
        public DownloadStatus Status { get; } = status;

        public Stream? ResultStream { get; } = resultStream;

        public Exception? Error { get; } = error;
    }
}
