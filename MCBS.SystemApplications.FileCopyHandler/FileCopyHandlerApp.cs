using MCBS.Application;
using MCBS.BlockForms.DialogBox;
using MCBS.BlockForms.FileSystem;
using MCBS.BlockForms.FileSystem.IO;
using MCBS.UI;
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

            FileCopyHandler fileCopyHandler = new(fileIOStreams);
            using CancellationTokenSource cancellationTokenSource = new();
            Task copyTask = fileCopyHandler.StartAsync(cancellationTokenSource.Token);

            try
            {
                if (!copyTask.Wait(100))
                    this.RunForm(new FileCopyHandlerForm(fileCopyHandler, copyTask, cancellationTokenSource));
                copyTask.Wait();
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
                stringBuilder.AppendLine("文件复制时遇到了错误：");
                stringBuilder.AppendLine(ExceptionMessageFactory.GetLocalMessag(ex.InnerException ?? ex));
                stringBuilder.AppendLine("目标文件路径：");
                stringBuilder.Append(fileCopyHandler.CurrentFile?.Destination?.Name ?? "-");

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

            return 0;
        }

        public void Exit()
        {
            throw new NotImplementedException();
        }
    }
}
