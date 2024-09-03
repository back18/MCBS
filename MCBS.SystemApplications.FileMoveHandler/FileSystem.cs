using QuanLib.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.SystemApplications.FileMoveHandler
{
    public class FileSystem
    {
        public static bool DriveEquals(string path1, string path2)
        {
            ArgumentException.ThrowIfNullOrEmpty(path1, nameof(path1));
            ArgumentException.ThrowIfNullOrEmpty(path2, nameof(path2));

            if (path1 == path2)
                return true;

            if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
                return UnixFileSystem.DriveEquals(path1, path2);

            path1 = Path.GetFullPath(path1);
            path2 = Path.GetFullPath(path2);

            string? drive1 = Path.GetPathRoot(path1);
            string? drive2 = Path.GetPathRoot(path2);
            return drive1 == drive2;
        }
    }
}
