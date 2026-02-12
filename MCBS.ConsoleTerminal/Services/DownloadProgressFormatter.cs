using QuanLib.Core;
using QuanLib.Downloader;
using QuanLib.TextFormat;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.ConsoleTerminal.Services
{
    public class DownloadProgressFormatter : IDownloadProgressFormatter
    {
        public DownloadProgressFormatter() : this(DefaultBytesFormatter, DefaultTimeFormatter) { }

        public DownloadProgressFormatter(BytesFormatter bytesFormatter, TimeFormatter timeFormatter)
        {
            ArgumentNullException.ThrowIfNull(bytesFormatter, nameof(bytesFormatter));
            ArgumentNullException.ThrowIfNull(timeFormatter, nameof(timeFormatter));

            _bytesFormatter = bytesFormatter;
            _timeFormatter = timeFormatter;
            ProgressBarLength = 20;
        }

        private static readonly BytesFormatter DefaultBytesFormatter = new(AbbreviationBytesUnitText.Default);
        private static readonly TimeFormatter DefaultTimeFormatter = new(AbbreviationTimeUnitText.Default);

        private readonly BytesFormatter _bytesFormatter;
        private readonly TimeFormatter _timeFormatter;

        public int ProgressBarLength { get; set; }

        public string FormatProgress(DownloadProgress progress)
        {
            StringBuilder stringBuilder = new();
            stringBuilder.Append(new ProgressBar((int)progress.TotalFileSize)
            {
                Current = (int)progress.DownloadedFileSize,
                Length = ProgressBarLength
            });

            stringBuilder.AppendFormat(
                " {0}/s - {1}/{2} - {3}",
                _bytesFormatter.Format((long)progress.BytesPerSecondSpeed),
                _bytesFormatter.Format(progress.DownloadedFileSize),
                _bytesFormatter.Format(progress.TotalFileSize),
                _timeFormatter.Format(progress.RemainingTime));

            return stringBuilder.ToString();
        }
    }
}
