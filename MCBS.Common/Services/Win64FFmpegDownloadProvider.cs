using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.Common.Services
{
    public class Win64FFmpegDownloadProvider : IFFmpegDownloadProvider
    {
        public string DownloadUrl { get; } = "https://github.com/BtbN/FFmpeg-Builds/releases/download/latest/ffmpeg-n7.1-latest-win64-gpl-shared-7.1.zip";

        public string PackName { get; } = "ffmpeg-n7.1-latest-win64-gpl-shared-7.1";
    }
}
