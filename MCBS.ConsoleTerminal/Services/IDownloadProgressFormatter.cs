using QuanLib.Downloader;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.ConsoleTerminal.Services
{
    public interface IDownloadProgressFormatter
    {
        public string FormatProgress(DownloadProgress progress);
    }
}
