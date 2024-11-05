using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.BlockForms.FileSystem
{
    public class FileSystemUtil
    {
        public static bool TryDeleteFile(string? path)
        {
            if (string.IsNullOrEmpty(path))
                return false;

            if (!File.Exists(path))
                return false;

            try
            {
                File.Delete(path);
            }
            catch
            {
                return false;
            }

            return true;
        }

        public static bool TryDeleteDirectory(string? path, bool recursive)
        {
            if (string.IsNullOrEmpty(path))
                return false;

            DirectoryInfo directoryInfo;
            try
            {
                directoryInfo = new(path);
            }
            catch
            {
                return false;
            }

            if (!directoryInfo.Exists)
                return false;

            if (!recursive && directoryInfo.GetFileSystemInfos().Length > 0)
                return false;

            try
            {
                directoryInfo.Delete(recursive);
            }
            catch
            {
                return false;
            }

            return true;
        }

        public static FileSystemInfo[] Sort(DirectoryInfo[] directoryInfos, FileInfo[] fileInfos, FileListSort fileListSort, ListSortDirection sortDirection)
        {
            ArgumentNullException.ThrowIfNull(directoryInfos, nameof(directoryInfos));
            ArgumentNullException.ThrowIfNull(fileInfos, nameof(fileInfos));

            FileSystemInfo[] result = new FileSystemInfo[directoryInfos.Length + fileInfos.Length];

            switch (fileListSort)
            {
                case FileListSort.FileName:
                    directoryInfos = directoryInfos.OrderBy(i => i.Name).ToArray();
                    fileInfos = fileInfos.OrderBy(i => i.Name).ToArray();
                    directoryInfos.CopyTo(result, 0);
                    fileInfos.CopyTo(result, directoryInfos.Length);
                    break;
                case FileListSort.FileSize:
                    directoryInfos = directoryInfos.OrderBy(i => i.Name).ToArray();
                    fileInfos = fileInfos.OrderBy(i => i.Length).ToArray();
                    directoryInfos.CopyTo(result, 0);
                    fileInfos.CopyTo(result, directoryInfos.Length);
                    break;
                case FileListSort.WriteTime:
                    directoryInfos = directoryInfos.OrderBy(i => i.LastWriteTime).ToArray();
                    fileInfos = fileInfos.OrderBy(i => i.LastWriteTime).ToArray();
                    directoryInfos.CopyTo(result, 0);
                    fileInfos.CopyTo(result, directoryInfos.Length);
                    break;
                default:
                    throw new InvalidEnumArgumentException(nameof(fileListSort), (int)fileListSort, typeof(FileListSort));
            }

            if (sortDirection == ListSortDirection.Descending)
                Array.Reverse(result);

            return result;
        }

        public static bool DriveEquals(string path1, string path2)
        {
            ArgumentException.ThrowIfNullOrEmpty(path1, nameof(path1));
            ArgumentException.ThrowIfNullOrEmpty(path2, nameof(path2));

            if (path1 == path2)
                return true;

            if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
                return UnixFileSystemUtil.DriveEquals(path1, path2);

            path1 = Path.GetFullPath(path1);
            path2 = Path.GetFullPath(path2);

            string? drive1 = Path.GetPathRoot(path1);
            string? drive2 = Path.GetPathRoot(path2);
            return drive1 == drive2;
        }
    }
}
