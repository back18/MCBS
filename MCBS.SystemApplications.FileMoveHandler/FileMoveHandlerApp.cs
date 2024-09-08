using MCBS.Application;
using MCBS.BlockForms.DialogBox;
using MCBS.BlockForms.FileSystem;
using MCBS.UI;
using QuanLib.Core.Extensions;
using QuanLib.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.SystemApplications.FileMoveHandler
{
    public class FileMoveHandlerApp : IProgram
    {
        public const string ID = "System.FileMoveHandler";

        public const string Name = "文件移动处理程序";

        public int Main(string[] args)
        {
            if (args.Length < 2)
                return -1;

            IForm? initiator = MinecraftBlockScreen.Instance.ProcessContextOf(this)?.Initiator;
            if (initiator is null)
                return -1;

            IOPath[] fileMovePaths, directoryMovePaths;
            try
            {
                string[] paths = ParsePaths(args);
                int endIndex = paths.Length - 1;
                string destDir = paths[endIndex];
                paths = paths.RemoveAt(endIndex);
                fileMovePaths = GetFileMovePaths(paths, destDir);
                directoryMovePaths = GetDirectoryMovePaths(paths, destDir);

                foreach (string sourceDir in directoryMovePaths.Select(i => i.Source))
                {
                    if (destDir.StartsWith(sourceDir))
                        throw new AggregateException("目标文件夹是源文件夹的子文件夹");
                }

                CreateDirectoryIfNotExists(destDir);
                foreach (IOPath directoryMovePath in directoryMovePaths)
                    CreateDirectoryIfNotExists(directoryMovePath.Destination);
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

            List<IOStream> fileMoveStreams = [];
            List<string> keepFiles = [];

            foreach (IOPath fileMovePath in fileMovePaths)
            {
                bool overwrite = false;
                FileStream? source = null;
                FileStream? destination = null;

                try
                {
                    if (File.Exists(fileMovePath.Destination))
                    {
                        if (DialogBoxHelper.OpenMessageBox(
                            initiator,
                            "是否覆盖",
                            $"存在同名文件“{fileMovePath.Destination}”",
                            MessageBoxButtons.Yes | MessageBoxButtons.No)
                            == MessageBoxButtons.Yes)
                            overwrite = true;
                        else
                            continue;
                    }

                    source = ReadSourceFileStream(fileMovePath.Source);
                    destination = CreateDestinationFileStream(fileMovePath.Destination, overwrite);
                    fileMoveStreams.Add(new(source, destination));
                }
                catch (AggregateException ex)
                {
                    source?.Close();
                    destination?.Close();
                    keepFiles.Add(fileMovePath.Source);

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

            if (fileMoveStreams.Count > 0)
            {
                FileMoveHandler fileMoveHandler = new(fileMoveStreams);
                using CancellationTokenSource cancellationTokenSource = new();
                Task moveTask = fileMoveHandler.StartAsync(cancellationTokenSource.Token);

                try
                {
                    if (!moveTask.Wait(100))
                        this.RunForm(new FileMoveHandlerForm(fileMoveHandler, cancellationTokenSource));
                    moveTask.Wait();
                }
                catch (AggregateException ex)
                {
                    StringBuilder stringBuilder = new();
                    stringBuilder.AppendLine("文件移动时遇到了错误：");
                    stringBuilder.AppendLine(ExceptionMessageFactory.GetLocalMessag(ex.InnerException ?? ex));
                    stringBuilder.AppendLine("目标文件路径：");
                    stringBuilder.Append(fileMoveHandler.CurrentFile?.Destination?.Name ?? "-");

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

            foreach (IOPath directoryMovePath in directoryMovePaths)
            {
                string directory = directoryMovePath.Source;

                if (keepFiles.Where(i => i.StartsWith(directory)).Any())
                    continue;

                if (Directory.Exists(directory))
                {
                    try
                    {
                        Directory.Delete(directory, true);
                    }
                    catch
                    {

                    }
                }
            }

            return 0;

            void ClearStreams()
            {
                foreach (IOStream fileMoveStream in fileMoveStreams)
                {
                    fileMoveStream.Source.Close();
                    fileMoveStream.Destination.Close();
                }
                fileMoveStreams.Clear();
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

        private static IOPath[] GetFileMovePaths(string[] paths, string destDir)
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

        private static IOPath[] GetDirectoryMovePaths(string[] paths, string destDir)
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
