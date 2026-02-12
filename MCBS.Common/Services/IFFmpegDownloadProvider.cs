using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.Common.Services
{
    public interface IFFmpegDownloadProvider
    {
        public string DownloadUrl { get; }

        public string PackName { get; }
    }
}
