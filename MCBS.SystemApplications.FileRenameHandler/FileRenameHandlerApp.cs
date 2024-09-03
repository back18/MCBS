using MCBS.Application;
using MCBS.BlockForms.DialogBox;
using MCBS.BlockForms.FileSystem;
using MCBS.UI;
using QuanLib.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.SystemApplications.FileRenameHandler
{
    public class FileRenameHandlerApp : IProgram
    {
        public const string ID = "System.FileRenameHandler";

        public const string Name = "文件重命名处理程序";

        public int Main(string[] args)
        {
            if (args.Length != 1)
                return -1;

            IForm? initiator = MinecraftBlockScreen.Instance.ProcessContextOf(this)?.Initiator;
            if (initiator is null)
                return -1;

            string path = Path.GetFullPath(args[0]);
            bool isDirectory;

            if (File.Exists(path))
            {
                isDirectory = false;
            }
            else if (Directory.Exists(path))
            {
                isDirectory = true;
            }
            else
            {
                ShowErrorDialogBox(initiator, $"给定的路径“{path}”既不是文件也不是文件夹");
                return -1;
            }

            string? directory = Path.GetDirectoryName(path);
            if (string.IsNullOrEmpty(directory))
            {
                ShowErrorDialogBox(initiator, $"无法重命名根目录“{path}”");
                return -1;
            }

            string name = DialogBoxHelper.OpenTextInputBox(initiator, "输入名称");
            if (string.IsNullOrEmpty(name))
                return -1;

            foreach (char c in Path.GetInvalidFileNameChars())
            {
                if (name.Contains(c))
                {
                    ShowErrorDialogBox(initiator, $"新名称“{name}”包含非法字符‘{c}’");
                    return -1;
                }
            }

            string newPath = Path.Combine(directory, name);
            Func<string, bool> existsHandler = isDirectory ? Directory.Exists : File.Exists;
            Action<string, string> renameHandler = isDirectory ? Directory.Move : File.Move;

            if (existsHandler.Invoke(newPath))
            {
                ShowErrorDialogBox(initiator, $"已存在名称为“{name}”的文件或文件夹");
                return -1;
            }

            try
            {
                renameHandler.Invoke(path, newPath);
            }
            catch (Exception ex)
            {
                StringBuilder stringBuilder = new();
                stringBuilder.AppendLine("重命名时遇到了错误：");
                stringBuilder.AppendLine(ExceptionMessageFactory.GetLocalMessag(ex));
                stringBuilder.AppendLine("路径：");
                stringBuilder.Append(newPath);

                DialogBoxHelper.OpenMessageBox(
                initiator,
                "错误",
                stringBuilder.ToString(),
                MessageBoxButtons.Yes);
                return -1;
            }

            return 0;
        }

        public void Exit()
        {
            throw new NotImplementedException();
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
