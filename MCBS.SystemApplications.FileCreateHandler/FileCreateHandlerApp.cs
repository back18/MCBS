using MCBS.Application;
using MCBS.BlockForms.DialogBox;
using MCBS.BlockForms.FileSystem;
using MCBS.UI;
using QuanLib.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.SystemApplications.FileCreateHandler
{
    public class FileCreateHandlerApp : IProgram
    {
        public const string ID = "System.FileCreateHandler";

        public const string Name = "文件新建处理程序";

        private const string FILE = nameof(FILE);

        private const string DIRECTORY = nameof(DIRECTORY);

        public int Main(string[] args)
        {
            bool isDirectory;
            if (args.Length == 1)
            {
                isDirectory = false;
            }
            else if (args.Length == 2)
            {
                switch (args[1])
                {
                    case FILE:
                    case "F":
                        isDirectory = false;
                        break;
                    case DIRECTORY:
                    case "D":
                        isDirectory = true;
                        break;
                    default:
                        return -1;
                }
            }
            else
            {
                return -1;
            }

            IForm? initiator = MinecraftBlockScreen.Instance.ProcessContextOf(this)?.Initiator;
            if (initiator is null)
                return -1;

            string path;
            try
            {
                path = ParsePath(args[0]);
            }
            catch (AggregateException aggregateException)
            {
                ShowErrorDialogBox(initiator, aggregateException.Message);
                return -1;
            }

            string? directory = Path.GetDirectoryName(path);
            if (string.IsNullOrEmpty(directory))
            {
                ShowErrorDialogBox(initiator, $"找不到路径“{path}”的父级目录");
                return -1;
            }

            string name = Path.GetFileName(path);
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                if (name.Contains(c))
                {
                    ShowErrorDialogBox(initiator, $"名称“{name}”包含非法字符‘{c}’");
                    return -1;
                }
            }

            Func<string, bool> existsHandler = isDirectory ? Directory.Exists : File.Exists;
            Action<string> createHandler = isDirectory ? CreateDirectoryIfNotExists : CreateFileIfNotExists;

            if (existsHandler.Invoke(path))
                path = isDirectory ? GetAvailableDirectoryName(directory, name) : GetAvailableFileName(directory, name);

            try
            {
                CreateDirectoryIfNotExists(directory);
                createHandler.Invoke(path);
            }
            catch (AggregateException aggregateException)
            {
                ShowErrorDialogBox(initiator, aggregateException.Message);
                return -1;
            }

            return 0;
        }

        public void Exit()
        {
            throw new NotImplementedException();
        }

        private static string ParsePath(string arg)
        {
            ArgumentNullException.ThrowIfNull(arg, nameof(arg));

            try
            {
                return Path.GetFullPath(arg);
            }
            catch (Exception ex)
            {
                StringBuilder stringBuilder = new();
                stringBuilder.AppendLine("解析路径参数时遇到了错误：");
                stringBuilder.AppendLine(ExceptionMessageFactory.GetLocalMessag(ex));
                stringBuilder.AppendLine("路径参数：");
                stringBuilder.Append(arg);
                throw new AggregateException(stringBuilder.ToString(), ex);
            }
        }

        private static string GetAvailableFileName(string directory, string name)
        {
            ArgumentException.ThrowIfNullOrEmpty(directory, nameof(directory));
            ArgumentException.ThrowIfNullOrEmpty(name, nameof(name));

            string fileExtension = Path.GetExtension(name);
            string fileName = Path.GetFileNameWithoutExtension(name);

            for (int i = 2; ; i++)
            {
                string newName = $"{fileName}({i}){fileExtension}";
                string newPath = Path.Combine(directory, newName);
                if (!File.Exists(newPath))
                    return newPath;
            }
        }

        private static string GetAvailableDirectoryName(string directory, string name)
        {
            ArgumentException.ThrowIfNullOrEmpty(directory, nameof(directory));
            ArgumentException.ThrowIfNullOrEmpty(name, nameof(name));

            for (int i = 2; ; i++)
            {
                string newName = $"{name}({i})";
                string newPath = Path.Combine(directory, newName);
                if (!Directory.Exists(newPath))
                    return newPath;
            }
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

        private static void CreateFileIfNotExists(string path)
        {
            if (!File.Exists(path))
            {
                try
                {
                    using FileStream fileStream = File.Create(path);
                }
                catch (Exception ex)
                {
                    StringBuilder stringBuilder = new();
                    stringBuilder.AppendLine("创建文件时遇到了错误：");
                    stringBuilder.AppendLine(ExceptionMessageFactory.GetLocalMessag(ex));
                    stringBuilder.AppendLine("文件路径：");
                    stringBuilder.Append(path);
                    throw new AggregateException(stringBuilder.ToString(), ex);
                }
            }
        }

        private static MessageBoxButtons ShowErrorDialogBox(IForm initiator, string message)
        {
            return DialogBoxHelper.OpenMessageBox(
                initiator,
                "错误",
                message,
                MessageBoxButtons.OK);
        }
    }
}
