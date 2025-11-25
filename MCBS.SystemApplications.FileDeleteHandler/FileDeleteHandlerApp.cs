using MCBS.Application;
using MCBS.BlockForms.DialogBox;
using MCBS.BlockForms.FileSystem;
using MCBS.UI;
using QuanLib.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.SystemApplications.FileDeleteHandler
{
    public class FileDeleteHandlerApp : IProgram
    {
        public const string Id = "System.FileDeleteHandler";

        public const string Name = "文件删除处理程序";

        public int Main(string[] args)
        {
            if (args.Length == 0)
                return -1;

            IForm? initiator = MinecraftBlockScreen.Instance.ProcessContextOf(this)?.Initiator;
            if (initiator is null)
                return -1;

            List<string> files = [];
            List<string> directorys = [];

            foreach (string arg in args)
            {
                try
                {
                    if (Directory.Exists(arg))
                    {
                        directorys.Add(arg);
                        directorys.AddRange(DirectoryUtil.GetAllDirectories(arg));
                        files.AddRange(DirectoryUtil.GetAllFiles(arg));
                    }
                    else if (File.Exists(arg))
                    {
                        files.Add(arg);
                    }
                }
                catch (Exception ex)
                {
                    StringBuilder stringBuilder = new();
                    stringBuilder.AppendLine("查找文件时遇到了错误：");
                    stringBuilder.AppendLine(ExceptionMessageFactory.GetLocalMessag(ex));
                    stringBuilder.AppendLine("路径参数：");
                    stringBuilder.Append(arg);

                    DialogBoxHelper.OpenMessageBox(
                    initiator,
                    "错误",
                    stringBuilder.ToString(),
                    MessageBoxButtons.Yes);
                    return -1;
                }
            }

            if (files.Count == 0 && directorys.Count == 0)
                return -1;

            StringBuilder warningMessage = new("是否永久删除选定的");
            if (directorys.Count > 0)
            {
                warningMessage.AppendFormat("{0}个文件夹", directorys.Count);
                if (files.Count > 0)
                    warningMessage.Append("共计");
            }
            if (files.Count > 0)
                warningMessage.AppendFormat("{0}个文件", files.Count);
            warningMessage.Append('？');

            if (ShowWarningDialogBox(initiator, warningMessage.ToString()) == MessageBoxButtons.No)
                return -1;

            FileDeleteHandler fileDeleteHandler = new(files);
            using CancellationTokenSource cancellationTokenSource = new();
            fileDeleteHandler.ThrowException += (sender, e) =>
            {
                if (DialogBoxHelper.OpenMessageBox(
                    initiator,
                    "是否跳过",
                    e.Argument.Message,
                    MessageBoxButtons.Yes | MessageBoxButtons.No)
                    == MessageBoxButtons.No)
                    cancellationTokenSource.Cancel();
            };
            Task deleteTask = fileDeleteHandler.StartAsync(cancellationTokenSource.Token);

            try
            {
                if (!deleteTask.Wait(100))
                    this.RunForm(new FileDeleteHandlerForm(fileDeleteHandler, deleteTask, cancellationTokenSource));
                deleteTask.Wait();
            }
            catch (AggregateException ex) when (ex.InnerException is TaskCanceledException)
            {
                DialogBoxHelper.OpenMessageBox(
                    initiator,
                    "提醒",
                    "任务已取消",
                    MessageBoxButtons.Yes);
                return 0;
            }

            foreach (string directory in directorys.Order().OrderByDescending(i => i.Length))
                FileSystemUtil.TryDeleteDirectory(directory, false);

            return 0;
        }

        public void Exit()
        {
            throw new NotImplementedException();
        }

        private static MessageBoxButtons ShowWarningDialogBox(IForm initiator, string message)
        {
            return DialogBoxHelper.OpenMessageBox(
                initiator,
                "警告",
                message,
                MessageBoxButtons.Yes | MessageBoxButtons.No);
        }
    }
}
