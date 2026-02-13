using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.Services
{
    public interface IFFmpegPathProvider
    {
        public DirectoryInfo FFmpeg { get; }

        public DirectoryInfo Bin { get; }

        public FileInfo Win64ZipFile { get; }

        public FileInfo Win64FileManifest { get; }
    }
}
