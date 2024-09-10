using QuanLib.TextFormat;
using QuanLib.Core;
using QuanLib.Downloader;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net.Core;
using Downloader;
using QuanLib.IO;
using System.Diagnostics.CodeAnalysis;
using QuanLib.Logging;

namespace MCBS
{
    public static class DownloadHelper
    {
        private static readonly LogImpl LOGGER = LogManager.Instance.GetLogger();

        public static bool ReadIfValid(string path, string hash, HashType hashType, [MaybeNullWhen(false)] out FileStream result)
        {
            if (File.Exists(path))
            {
                FileStream fileStream = File.OpenRead(path);
                if (HashUtil.GetHashString(fileStream, hashType) == hash)
                {
                    result = fileStream;
                    return true;
                }
                else
                {
                    fileStream.Close();
                }
            }

            result = null;
            return false;
        }

        public static async Task<Stream> ReadOrDownloadAsync(string url, string path, string hash, HashType hashType)
        {
            ArgumentException.ThrowIfNullOrEmpty(url, nameof(url));
            ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));
            ArgumentException.ThrowIfNullOrEmpty(hash, nameof(hash));

            if (ReadIfValid(path, hash, hashType, out var fileStream))
                return fileStream;

            return await DownloadAsync(url, path);
        }

        public static async Task<Stream> DownloadAsync(string url, string? path = null)
        {
            ArgumentException.ThrowIfNullOrEmpty(url, nameof(url));

            start:
            LOGGER.Info("开始下载: " + url);
            DownloadTask downloadTask = new(url, path);
            Task<Stream?> task = downloadTask.StartAsync();

            while (!task.IsCompleted)
            {
                if (!downloadTask.DownloadProgressAvailable)
                {
                    await Task.Delay(10);
                    continue;
                }

                LOGGER.Info(FormatProgress(downloadTask.DownloadProgressChangedEventArgs));

                try
                {
                    await task.WaitAsync(TimeSpan.FromSeconds(1));
                }
                catch (TimeoutException)
                {

                }
            }

            Stream? result = await task;

            if (result is null || downloadTask.Download.Status == DownloadStatus.Failed)
            {
                LOGGER.Warn("下载失败，即将重试");
                await Task.Delay(1000);
                goto start;
            }
            if (downloadTask.DownloadProgressAvailable)
                LOGGER.Info(FormatProgress(downloadTask.DownloadProgressChangedEventArgs));

            return result;
        }

        private static string FormatProgress(DownloadProgressChangedEventArgs e)
        {
            ArgumentNullException.ThrowIfNull(e, nameof(e));

            BytesFormatter bytesFormatter = new(AbbreviationBytesUnitText.Default);
            TimeFormatter timeFormatter = new(AbbreviationTimeUnitText.Default);
            StringBuilder stringBuilder = new();
            stringBuilder.Append(new ProgressBar((int)e.TotalBytesToReceive)
            {
                Current = (int)e.ReceivedBytesSize,
                Length = 20
            });
            stringBuilder.AppendFormat(
                " {0}/s - {1}/{2} - {3}",
                bytesFormatter.Format((long)e.BytesPerSecondSpeed),
                bytesFormatter.Format(e.ReceivedBytesSize),
                bytesFormatter.Format(e.TotalBytesToReceive),
                timeFormatter.Format(e.BytesPerSecondSpeed == 0 ? TimeSpan.Zero : TimeSpan.FromSeconds((e.TotalBytesToReceive - e.ReceivedBytesSize) / e.BytesPerSecondSpeed)));

            return stringBuilder.ToString();
        }
    }
}
