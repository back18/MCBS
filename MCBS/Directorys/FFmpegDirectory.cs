using QuanLib.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Directorys
{
    public class FFmpegDirectory : DirectoryBase
    {
        public FFmpegDirectory(string directory) : base(directory)
        {
            BinDir = AddDirectory<BinDirectory>("bin");
            FFmpegWin64ZipFile = Combine("ffmpeg-n6.1-latest-win64-gpl-shared-6.1.zip");
            FFmpegWin64IndexFile = Combine("ffmpeg-n6.1-latest-win64-gpl-shared-6.1.json");
        }

        public BinDirectory BinDir { get; }

        public string FFmpegWin64ZipFile { get; }

        public string FFmpegWin64IndexFile { get; }

        public class BinDirectory : DirectoryBase
        {
            public BinDirectory(string directory) : base(directory)
            {
            }
        }
    }
}
