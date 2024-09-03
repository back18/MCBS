using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.BlockForms.FileSystem
{
    public static class FileListUtil
    {
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
    }
}
