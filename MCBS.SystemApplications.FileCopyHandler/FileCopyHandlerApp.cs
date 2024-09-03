using MCBS.Application;
using MCBS.BlockForms.DialogBox;
using MCBS.BlockForms.FileSystem;
using MCBS.UI;
using QuanLib.Core.Extensions;
using QuanLib.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.SystemApplications.FileCopyHandler
{
    public class FileCopyHandlerApp : IProgram
    {
        public const string ID = "System.FileCopyHandler";

        public const string Name = "文件复制处理程序";

        public int Main(string[] args)
        {
            if (args.Length < 2)
                return -1;

            IForm? initiator = MinecraftBlockScreen.Instance.ProcessContextOf(this)?.Initiator;
            if (initiator is null)
                return -1;

            IOPath[] fileCopyPaths, directoryCopyPaths;
            try
            {
                string[] paths = ParsePaths(args);
                int endIndex = paths.Length - 1;
                string destDir = paths[endIndex];
                paths = paths.RemoveAt(endIndex);
                fileCopyPaths = GetFileCopyPaths(paths, destDir);
                directoryCopyPaths = GetDirectoryCopyPaths(paths, destDir);

                foreach (IOPath directoryCopyPath in directoryCopyPaths)
                    CreateDirectoryIfNotExists(directoryCopyPath.Destination);
            }
            catch (AggregateException aggregateException)
            {
                DialogBoxHelper.OpenMessageBox(
                    initiator,
                    "错误",
                    aggregateException.Message,
                    MessageBoxButtons.Yes);
                return -1;
            }

            List<IOStream> fileCopyStreams = [];
            foreach (IOPath fileCopyPath in fileCopyPaths)
            {
                bool overwrite = false;
                FileStream? source = null;
                FileStream? destination = null;

                try
                {
                    if (File.Exists(fileCopyPath.Destination))
                    {
                        if (DialogBoxHelper.OpenMessageBox(
                            initiator,
                            "是否覆盖",
                            $"存在同名文件“{fileCopyPath.Destination}”",
                            MessageBoxButtons.Yes | MessageBoxButtons.No)
                            == MessageBoxButtons.Yes)
                            overwrite = true;
                        else
                            continue;
                    }

                    source = ReadSourceFileStream(fileCopyPath.Source);
                    destination = CreateDestinationFileStream(fileCopyPath.Destination, overwrite);
                    fileCopyStreams.Add(new(source, destination));
                }
                catch (AggregateException ex)
                {
                    source?.Close();
                    destination?.Close();

                    if (DialogBoxHelper.OpenMessageBox(
                        initiator,
                        "是否跳过",
                        ex.Message,
                        MessageBoxButtons.Yes | MessageBoxButtons.No) == MessageBoxButtons.No)
                    {
                        ClearStreams();
                        return -1;
                    }
                }
            }

            if (fileCopyStreams.Count > 0)
            {
                FileCopyHandler fileCopyHandler = new(fileCopyStreams);
                using CancellationTokenSource cancellationTokenSource = new();
                Task copyTask = fileCopyHandler.StartAsync(cancellationTokenSource.Token);

                try
                {
                    if (!copyTask.Wait(100))
                        this.RunForm(new FileCopyHandlerForm(fileCopyHandler, cancellationTokenSource));
                    copyTask.Wait();
                }
                catch (AggregateException ex)
                {
                    StringBuilder stringBuilder = new();
                    stringBuilder.AppendLine("文件复制时遇到了错误：");
                    stringBuilder.AppendLine(ExceptionMessageFactory.GetLocalMessag(ex.InnerException ?? ex));
                    stringBuilder.AppendLine("目标文件路径：");
                    stringBuilder.Append(fileCopyHandler.CurrentFile?.Destination?.Name ?? "-");

                    DialogBoxHelper.OpenMessageBox(
                        initiator,
                        "错误",
                        stringBuilder.ToString(),
                        MessageBoxButtons.Yes);
                }
                finally
                {
                    ClearStreams();
                }
            }

            return 0;

            void ClearStreams()
            {
                foreach (IOStream fileCopyStream in fileCopyStreams)
                {
                    fileCopyStream.Source.Close();
                    fileCopyStream.Destination.Close();
                }
                fileCopyStreams.Clear();
            }
        }

        public void Exit()
        {
            throw new NotImplementedException();
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
                    stringBuilder.Append(args[i]);
                    throw new AggregateException(stringBuilder.ToString(), ex);
                }
            }

            return result;
        }

        private static IOPath[] GetFileCopyPaths(string[] paths, string destDir)
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
                    stringBuilder.Append(sourcePath);
                    throw new AggregateException(stringBuilder.ToString(), ex);
                }
            }

            return result.ToArray();
        }

        private static IOPath[] GetDirectoryCopyPaths(string[] paths, string destDir)
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
                    stringBuilder.Append(sourcePath);
                    throw new AggregateException(stringBuilder.ToString(), ex);
                }
            }

            return result.ToArray();
        }

        private static void CreateDirectoryIfNotExists(string path)
        {
            if (!Directory.Exists(path))
            {
                try
                {
                    Directory.CreateDirectory(path);
                }
                catch (Exception ex)
                {
                    StringBuilder stringBuilder = new();
                    stringBuilder.AppendLine("创建目录时遇到了错误：");
                    stringBuilder.AppendLine(ExceptionMessageFactory.GetLocalMessag(ex));
                    stringBuilder.AppendLine("目录路径：");
                    stringBuilder.Append(path);
                    throw new AggregateException(stringBuilder.ToString(), ex);
                }
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
                stringBuilder.Append(sourceFile);
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
                stringBuilder.Append(destinationFile);
                throw new AggregateException(stringBuilder.ToString(), ex);
            }
        }
    }
}
