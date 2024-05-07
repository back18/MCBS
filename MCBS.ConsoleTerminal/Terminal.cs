using QuanLib.Commands;
using QuanLib.Commands.CommandLine;
using QuanLib.Core;
using QuanLib.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.ConsoleTerminal
{
    public abstract class Terminal : RunnableBase
    {
        public Terminal(CommandSender commandSender) : base(LogManager.Instance.LoggerGetter)
        {
            ArgumentNullException.ThrowIfNull(commandSender, nameof(commandSender));

            _commandSender = commandSender;
            CommandManager = new();
            RegisterCommands();
        }

        protected readonly CommandSender _commandSender;

        public CommandManager CommandManager { get; }

        protected abstract CommandReaderResult ReadCommand();

        protected override void Run()
        {
            while (IsRunning)
            {
                CommandReaderResult result = ReadCommand();

                if (!IsRunning)
                {
                    break;
                }

                if (!MinecraftBlockScreen.IsInstanceLoaded || !MinecraftBlockScreen.Instance.IsRunning)
                {
                    Console.WriteLine("MCBS未在运行，无法执行命令");
                    continue;
                }

                if (result.Command is null)
                {
                    Console.WriteLine("未知或不完整命令");
                    continue;
                }

                try
                {
                    string message = result.Command.Execute(_commandSender, result.Args.ToArray());
                    Console.WriteLine(message);
                }
                catch (AggregateException aggregateException)
                {
                    foreach (Exception innerException in aggregateException.InnerExceptions)
                        Console.WriteLine(ObjectFormatter.Format(innerException));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ObjectFormatter.Format(ex));
                }
            }
        }

        private void RegisterCommands()
        {
            CommandManager.Register(
                new CommandBuilder()
                .On("application list")
                .Allow(PrivilegeLevel.User)
                .Execute(GetApplicationList)
                .Build());

            CommandManager.Register(
                new CommandBuilder()
                .On("screen list")
                .Allow(PrivilegeLevel.User)
                .Execute(GetScreenList)
                .Build());

            CommandManager.Register(
                new CommandBuilder()
                .On("process list")
                .Allow(PrivilegeLevel.User)
                .Execute(GetProcessList)
                .Build());

            CommandManager.Register(
                new CommandBuilder()
                .On("form list")
                .Allow(PrivilegeLevel.User)
                .Execute(GetFormList)
                .Build());

            CommandManager.Register(
                new CommandBuilder()
                .On("screen builder")
                .Allow(PrivilegeLevel.User)
                .Execute(GetScreenList)
                .Build());

            CommandManager.Register(
                new CommandBuilder()
                .On("frame count")
                .Allow(PrivilegeLevel.User)
                .Execute(GetFrameCount)
                .Build());
        }

        #region commands

        private static string GetApplicationList()
        {
            var list = MinecraftBlockScreen.Instance.AppComponents;
            StringBuilder sb = new();
            sb.AppendLine($"当前已加载{list.Count}个应用程序，应用程序列表：");
            foreach (var applicationManifest in list.Values)
                sb.AppendLine(applicationManifest.ToString());

            return sb.ToString().TrimEnd();
        }

        private static string GetScreenList()
        {
            var list = MinecraftBlockScreen.Instance.ScreenManager.Items;
            StringBuilder sb = new();
            sb.AppendLine($"当前已加载{list.Count}个屏幕，屏幕列表：");
            foreach (var context in list.Values)
                sb.AppendLine(context.ToString());

            return sb.ToString().TrimEnd();
        }

        private static string SetScreenBuilderEnable(bool enable)
        {
            MinecraftBlockScreen.Instance.ScreenBuildManager.Enable = enable;

            if (enable)
                return "屏幕构造器已启用";
            else
                return "屏幕构造器已禁用";
        }

        private static string GetProcessList()
        {
            var list = MinecraftBlockScreen.Instance.ProcessManager.Items;
            StringBuilder sb = new();
            sb.AppendLine($"当前已启动{list.Count}个进程，进程列表：");
            foreach (var context in list.Values)
                sb.AppendLine(context.ToString());

            return sb.ToString().TrimEnd();
        }

        private static string GetFormList()
        {
            var list = MinecraftBlockScreen.Instance.FormManager.Items;
            StringBuilder sb = new();
            sb.AppendLine($"当前已打开{list.Count}个窗体，窗体列表：");
            foreach (var context in list.Values)
                sb.AppendLine(context.ToString());

            return sb.ToString().TrimEnd();
        }

        private static int GetFrameCount()
        {
            return MinecraftBlockScreen.Instance.SystemTick;
        }

        #endregion
    }
}
