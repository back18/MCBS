using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.Services
{
    public class FFmpegPathProvider : IFFmpegPathProvider
    {
        public DirectoryInfo FFmpeg => McbsPathManager.MCBS_FFmpeg;

        public DirectoryInfo Bin => McbsPathManager.MCBS_FFmpeg_Bin;

        public FileInfo Win64ZipFile => McbsPathManager.MCBS_FFmpeg_Win64ZipFile;

        public FileInfo Win64FileManifest => McbsPathManager.MCBS_FFmpeg_Win64FileManifest;
    }
}
