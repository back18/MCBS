using MCBS.Application;
using MCBS.BlockForms.DialogBox;
using MCBS.BlockForms.FileSystem;
using MCBS.BlockForms.FileSystem.IO;
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

            FileIOHandler fileIOHandler = new(initiator, true);
            IOList ioList;
            try
            {
                ioList = fileIOHandler.Start(args);
            }
            catch (TaskCanceledException)
            {
                return -1;
            }

            IOStream[] fileIOStreams = ioList.FileIOStreams;
            if (fileIOStreams.Length == 0)
                return 0;

            FileMoveHandler fileMoveHandler = new(fileIOStreams);
            using CancellationTokenSource cancellationTokenSource = new();
            Task moveTask = fileMoveHandler.StartAsync(cancellationTokenSource.Token);

            try
            {
                if (!moveTask.Wait(100))
                    this.RunForm(new FileMoveHandlerForm(fileMoveHandler, moveTask, cancellationTokenSource));
                moveTask.Wait();
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
            catch (AggregateException ex)
            {
                StringBuilder stringBuilder = new();
                stringBuilder.AppendLine("文件移动时遇到了错误：");
                stringBuilder.AppendLine(ExceptionMessageFactory.GetLocalMessag(ex.InnerException ?? ex));
                stringBuilder.AppendLine("目标文件路径：");
                stringBuilder.Append(fileMoveHandler.CurrentFile);

                DialogBoxHelper.OpenMessageBox(
                    initiator,
                    "错误",
                    stringBuilder.ToString(),
                    MessageBoxButtons.Yes);
                return -1;
            }
            finally
            {
                foreach (IOStream fileIOStream in fileIOStreams)
                {
                    fileIOStream.Source.Close();
                    fileIOStream.Destination.Close();
                }
            }

            foreach (IOStream fileIOStream in fileMoveHandler.GetCompleted())
                FileSystemUtil.TryDeleteFile(fileIOStream.Source.Name);

            foreach (string directory in ioList.DirectoryIOPaths.Select(i => i.Source).Order().OrderByDescending(i => i.Length))
                FileSystemUtil.TryDeleteDirectory(directory, false);

            return 0;
        }

        public void Exit()
        {
            throw new NotImplementedException();
        }
    }
}
