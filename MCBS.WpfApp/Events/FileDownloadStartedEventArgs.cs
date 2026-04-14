using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.WpfApp.Events
{
    public class FileDownloadStartedEventArgs(string url, string? filePath, long totalBytesToReceive) : EventArgs
    {
        public string Url { get; } = url;

        public string? FilePath { get; } = filePath;

        public long TotalBytesToReceive { get; } = totalBytesToReceive;
    }
}
