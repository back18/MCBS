using MCBS.BlockForms.DialogBox;
using MCBS.UI;
using QuanLib.Core.Extensions;
using QuanLib.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.BlockForms.FileSystem.IO
{
    public class FileIOHandler
    {
        public FileIOHandler(IForm initiator, bool allowNesting)
        {
            ArgumentNullException.ThrowIfNull(initiator, nameof(initiator));

            _initiator = initiator;
            _allowNesting = allowNesting;
        }

        private readonly IForm _initiator;

        private readonly bool _allowNesting;

        public IOList Start(string[] args)
        {
            IOPath[] fileIOPaths, directoryIOPaths;
            List<string> createdDirectorys = [];
            try
            {
                string[] paths = ParsePaths(args);
                int endIndex = paths.Length - 1;
                string destDir = paths[endIndex];
                paths = paths.RemoveAt(endIndex);
                fileIOPaths = GetFileIOPaths(paths, destDir);
                directoryIOPaths = GetDirectoryIOPaths(paths, destDir);

                if (!_allowNesting && directoryIOPaths.Select(i => i.Source).Any(destDir.StartsWith))
                    throw new AggregateException("目标文件夹是源文件夹的子文件夹");

                if (CreateDirectoryIfNotExists(destDir))
                    createdDirectorys.Add(destDir);

                foreach (IOPath directoryIOPath in directoryIOPaths)
                {
                    if (CreateDirectoryIfNotExists(directoryIOPath.Destination))
                        createdDirectorys.Add(directoryIOPath.Destination);
                }
            }
            catch (AggregateException aggregateException)
            {
                DialogBoxHelper.OpenMessageBox(
                    _initiator,
                    "错误",
                    aggregateException.Message,
                    MessageBoxButtons.Yes);
                throw new TaskCanceledException();
            }

            List<IOStream> fileIOStreams = [];
            foreach (IOPath fileIOPath in fileIOPaths)
            {
                bool overwrite = false;
                FileStream? source = null;
                FileStream? destination = null;

                try
                {
                    if (File.Exists(fileIOPath.Destination))
                    {
                        if (DialogBoxHelper.OpenMessageBox(
                            _initiator,
                            "是否覆盖",
                            $"存在同名文件“{fileIOPath.Destination}”",
                            MessageBoxButtons.Yes | MessageBoxButtons.No)
                            == MessageBoxButtons.Yes)
                            overwrite = true;
                        else
                            continue;
                    }

                    source = ReadSourceFileStream(fileIOPath.Source);
                    destination = CreateDestinationFileStream(fileIOPath.Destination, overwrite);
                    fileIOStreams.Add(new(source, destination));
                }
                catch (AggregateException ex)
                {
                    source?.Close();
                    destination?.Close();

                    if (DialogBoxHelper.OpenMessageBox(
                        _initiator,
                        "是否跳过",
                        ex.Message,
                        MessageBoxButtons.Yes | MessageBoxButtons.No) == MessageBoxButtons.No)
                    {
                        foreach (IOStream fileIOStream in fileIOStreams)
                        {
                            fileIOStream.Source.Close();
                            fileIOStream.Destination.Close();
                            FileSystemUtil.TryDeleteFile(fileIOStream.Destination.Name);
                        }

                        createdDirectorys.Reverse();
                        foreach (string directory in createdDirectorys.Order().OrderByDescending(i => i.Length))
                            FileSystemUtil.TryDeleteDirectory(directory, false);

                        throw new TaskCanceledException();
                    }
                }
            }

            return new IOList()
            {
                FileIOPaths = fileIOPaths,
                DirectoryIOPaths = directoryIOPaths,
                FileIOStreams = fileIOStreams.ToArray()
            };
        }

        private static string[] ParsePaths(string[] args)
        {
            ArgumentNullException.ThrowIfNull(args, nameof(args));

            string[] result = new string[args.Length];

            for (int i = 0; i < args.Length; i++)
            {
                try
                {
                    result[i] = Path.GetFullPath(args[i]);
                }
                catch (Exception ex)
                {
                    StringBuilder stringBuilder = new();
                    stringBuilder.AppendLine("解析路径参数时遇到了错误：");
                    stringBuilder.AppendLine(ExceptionMessageFactory.GetLocalMessag(ex));
                    stringBuilder.AppendLine("路径参数：");
                    stringBuilder.AppendLine(args[i]);
                    stringBuilder.AppendLine("错误信息：");
                    throw new AggregateException(stringBuilder.ToString(), ex);
                }
            }

            return result;
        }

        private static IOPath[] GetFileIOPaths(string[] paths, string destDir)
        {
            ArgumentNullException.ThrowIfNull(paths, nameof(paths));

            List<IOPath> result = [];

            for (int i = 0; i < paths.Length; i++)
            {
                string sourcePath = paths[i];

                try
                {
                    string? baseDir = Path.GetDirectoryName(sourcePath);
                    if (string.IsNullOrEmpty(baseDir))
                        continue;

                    List<string> files = [];
                    if (Directory.Exists(sourcePath))
                        files.AddRange(DirectoryUtil.GetAllFiles(sourcePath));
                    else
                        files.Add(sourcePath);

                    baseDir = baseDir.TrimEnd(Path.DirectorySeparatorChar);
                    foreach (string file in files)
                    {
                        string subPath = file[(baseDir.Length + 1)..];
                        string path = Path.Combine(destDir, subPath);
                        result.Add(new(file, path));
                    }
                }
                catch (Exception ex)
                {
                    StringBuilder stringBuilder = new();
                    stringBuilder.AppendLine("查找文件时遇到了错误：");
                    stringBuilder.AppendLine(ExceptionMessageFactory.GetLocalMessag(ex));
                    stringBuilder.AppendLine("路径参数：");
                    stringBuilder.AppendLine(sourcePath);
                    stringBuilder.AppendLine("错误信息：");
                    throw new AggregateException(stringBuilder.ToString(), ex);
                }
            }

            return result.OrderBy(i => i.Destination).OrderBy(i => i.Destination.Length).ToArray();
        }

        private static IOPath[] GetDirectoryIOPaths(string[] paths, string destDir)
        {
            ArgumentNullException.ThrowIfNull(paths, nameof(paths));

            List<IOPath> result = [];

            for (int i = 0; i < paths.Length; i++)
            {
                string sourcePath = paths[i];
                if (!Directory.Exists(sourcePath))
                    continue;

                try
                {
                    string? baseDir = Path.GetDirectoryName(sourcePath);
                    if (string.IsNullOrEmpty(baseDir))
                        continue;

                    List<string> directorys = [sourcePath];
                    directorys.AddRange(DirectoryUtil.GetAllDirectories(sourcePath));

                    baseDir = baseDir.TrimEnd(Path.DirectorySeparatorChar);
                    foreach (string directory in directorys)
                    {
                        string subPath = directory[(baseDir.Length + 1)..];
                        string path = Path.Combine(destDir, subPath);
                        result.Add(new(directory, path));
                    }
                }
                catch (Exception ex)
                {
                    StringBuilder stringBuilder = new();
                    stringBuilder.AppendLine("查找目录时遇到了错误：");
                    stringBuilder.AppendLine(ExceptionMessageFactory.GetLocalMessag(ex));
                    stringBuilder.AppendLine("路径参数：");
                    stringBuilder.AppendLine(sourcePath);
                    stringBuilder.AppendLine("错误信息：");
                    throw new AggregateException(stringBuilder.ToString(), ex);
                }
            }

            return result.OrderBy(i => i.Destination).OrderBy(i => i.Destination.Length).ToArray();
        }

        private static bool CreateDirectoryIfNotExists(string path)
        {
            if (Directory.Exists(path))
                return false;

            try
            {
                Directory.CreateDirectory(path);
                return true;
            }
            catch (Exception ex)
            {
                StringBuilder stringBuilder = new();
                stringBuilder.AppendLine("创建目录时遇到了错误：");
                stringBuilder.AppendLine(ExceptionMessageFactory.GetLocalMessag(ex));
                stringBuilder.AppendLine("目录路径：");
                stringBuilder.AppendLine(path);
                stringBuilder.AppendLine("错误信息：");
                throw new AggregateException(stringBuilder.ToString(), ex);
            }
        }

        private static FileStream ReadSourceFileStream(string sourceFile)
        {
            ArgumentException.ThrowIfNullOrEmpty(sourceFile, nameof(sourceFile));

            try
            {
                return new(sourceFile, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            catch (Exception ex)
            {
                StringBuilder stringBuilder = new();
                stringBuilder.AppendLine("读取源文件时遇到了错误：");
                stringBuilder.AppendLine(ExceptionMessageFactory.GetLocalMessag(ex));
                stringBuilder.AppendLine("文件路径：");
                stringBuilder.AppendLine(sourceFile);
                stringBuilder.AppendLine("错误信息：");
                throw new AggregateException(stringBuilder.ToString(), ex);
            }
        }

        private static FileStream CreateDestinationFileStream(string destinationFile, bool overwrite)
        {
            ArgumentException.ThrowIfNullOrEmpty(destinationFile, nameof(destinationFile));

            FileMode fileMode = overwrite ? FileMode.Create : FileMode.CreateNew;

            try
            {
                return new(destinationFile, fileMode, FileAccess.Write, FileShare.None);
            }
            catch (Exception ex)
            {
                StringBuilder stringBuilder = new();
                stringBuilder.AppendLine("创建目标文件时遇到了错误：");
                stringBuilder.AppendLine(ExceptionMessageFactory.GetLocalMessag(ex));
                stringBuilder.AppendLine("文件路径：");
                stringBuilder.AppendLine(destinationFile);
                stringBuilder.AppendLine("错误信息：");
                throw new AggregateException(stringBuilder.ToString(), ex);
            }
        }
    }
}
