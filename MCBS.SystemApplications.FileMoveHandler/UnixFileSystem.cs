using QuanLib.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.SystemApplications.FileMoveHandler
{
    [SupportedOSPlatform("Linux")]
    [SupportedOSPlatform("MacOS")]
    public static class UnixFileSystem
    {
        public static bool DriveEquals(string path1, string path2)
        {
            ArgumentException.ThrowIfNullOrEmpty(path1, nameof(path1));
            ArgumentException.ThrowIfNullOrEmpty(path2, nameof(path2));
            ThrowHelper.FileNotFound(path1);
            ThrowHelper.FileNotFound(path2);

            if (path1 == path2)
                return true;

            path1 = Path.GetFullPath(path1);
            path2 = Path.GetFullPath(path2);

            if (Stat(path1, out FileStatus fileStatus1) != 0 || Stat(path2, out FileStatus fileStatus2) != 0)
                return false;

            return fileStatus1.st_dev == fileStatus2.st_dev;
        }

        [DllImport("libc", EntryPoint = "stat", CharSet = CharSet.Unicode)]
        private static extern int Stat(string path, out FileStatus buffer);

        [StructLayout(LayoutKind.Sequential)]
        private struct FileStatus
        {
            public required uint st_dev;
            public required uint st_ino;
            public required uint st_mode;
            public required uint st_nlink;
            public required uint st_uid;
            public required uint st_gid;
            public required uint st_rdev;
            public required long st_size;
            public required long st_blksize;
            public required long st_blocks;
            public required DateTime st_atim;
            public required DateTime st_mtim;
            public required DateTime st_ctim;
        }
    }
}
