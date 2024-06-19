using MCBS.Analyzer;
using MCBS.ConsoleTerminal.Extensions;
using QuanLib.Commands;
using QuanLib.Commands.CommandLine;
using QuanLib.Consoles;
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
                .On("screen forms")
                .Allow(PrivilegeLevel.User)
                .Execute(GetFormsOfScreen)
                .Build());

            CommandManager.Register(
                new CommandBuilder()
                .On("screen builder enable")
                .Allow(PrivilegeLevel.User)
                .Execute(EnableScreenBuilder)
                .Build());

            CommandManager.Register(
                new CommandBuilder()
                .On("screen builder disable")
                .Allow(PrivilegeLevel.User)
                .Execute(DisableScreenBuilder)
                .Build());

            CommandManager.Register(
                new CommandBuilder()
                .On("mcbs SystemTick")
                .Allow(PrivilegeLevel.User)
                .Execute(GetSystemTick)
                .Build());

            CommandManager.Register(
                new CommandBuilder()
                .On("analyzer mspt")
                .Allow(PrivilegeLevel.User)
                .Execute<Action>(MsptAnalysis)
                .Build());

            foreach (SystemStage stage in Enum.GetValues<SystemStage>())
            {
                CommandManager.Register(
                    new CommandBuilder()
                    .On(["analyzer", "mspt", stage.ToString()])
                    .Allow(PrivilegeLevel.User)
                    .Execute(() => MsptAnalysis(stage))
                    .Build());
            }
        }

        #region commands

        private static string GetApplicationList()
        {
            var list = MinecraftBlockScreen.Instance.AppComponents;

            StringBuilder stringBuilder = new();
            stringBuilder.AppendLine($"当前已加载{list.Count}个应用程序，应用程序列表：");
            foreach (var applicationManifest in list.Values)
                stringBuilder.AppendLine(applicationManifest.ToString());
            stringBuilder.Length -= Environment.NewLine.Length;

            return stringBuilder.ToString();
        }

        private static string GetScreenList()
        {
            var list = MinecraftBlockScreen.Instance.ScreenManager.Items;

            StringBuilder stringBuilder = new();
            stringBuilder.AppendLine($"当前已加载{list.Count}个屏幕，屏幕列表：");
            foreach (var context in list.Values)
                stringBuilder.AppendLine(context.ToString());
            stringBuilder.Length -= Environment.NewLine.Length;

            return stringBuilder.ToString();
        }

        private static string GetProcessList()
        {
            var list = MinecraftBlockScreen.Instance.ProcessManager.Items;

            StringBuilder stringBuilder = new();
            stringBuilder.AppendLine($"当前已启动{list.Count}个进程，进程列表：");
            foreach (var context in list.Values)
                stringBuilder.AppendLine(context.ToString());
            stringBuilder.Length -= Environment.NewLine.Length;

            return stringBuilder.ToString();
        }

        private static string GetFormList()
        {
            var list = MinecraftBlockScreen.Instance.FormManager.Items;

            StringBuilder stringBuilder = new();
            stringBuilder.AppendLine($"当前已打开{list.Count}个窗体，窗体列表：");
            foreach (var context in list.Values)
                stringBuilder.AppendLine(context.ToString());
            stringBuilder.Length -= Environment.NewLine.Length;

            return stringBuilder.ToString();
        }

        private static string GetFormsOfScreen(Guid screenGUID)
        {
            if (!MinecraftBlockScreen.Instance.ScreenManager.Items.TryGetValue(screenGUID, out var screenContext))
                return $"找不到GUID为 {screenGUID} 的屏幕";

            var list = MinecraftBlockScreen.Instance.FormManager.Items.Values.Where(s => s.RootForm == screenContext.RootForm);

            StringBuilder stringBuilder = new();
            stringBuilder.AppendLine($"屏幕({screenGUID})当前已打开{list.Count()}个窗体，窗体列表：");
            foreach (var context in list)
                stringBuilder.AppendLine(context.ToString());
            stringBuilder.Length -= Environment.NewLine.Length;

            return stringBuilder.ToString();
        }

        private static string EnableScreenBuilder()
        {
            MinecraftBlockScreen.Instance.ScreenBuildManager.Enable = true;
            return "屏幕构造器已启用";
        }

        private static string DisableScreenBuilder()
        {
            MinecraftBlockScreen.Instance.ScreenBuildManager.Enable = false;
            return "屏幕构造器已禁用";
        }

        private static string GetSystemTick()
        {
            return $"MCBS已运行{MinecraftBlockScreen.Instance.SystemTick}个Tick";
        }

        private static void MsptAnalysis()
        {
            LogManager.Instance.DisableConsoleOutput();
            Console.CursorVisible = false;

            ConsoleDynamicViwe consoleDynamicViwe = new(MinecraftBlockScreen.Instance.MsptAnalyzer.ToConsoleViwe, 50);
            consoleDynamicViwe.Start();
            ConsoleUtil.WaitForInputKey(ConsoleKey.Enter, 10);
            consoleDynamicViwe.Stop();

            Console.CursorVisible = true;
            LogManager.Instance.EnableConsoleOutput();
        }

        private static void MsptAnalysis(SystemStage stage)
        {
            LogManager.Instance.DisableConsoleOutput();
            Console.CursorVisible = false;

            do
            {
                Console.WriteLine(
                    string.Format("Tick{0}: {1}ms",
                    MinecraftBlockScreen.Instance.SystemTick,
                    Math.Round(MinecraftBlockScreen.Instance.MsptAnalyzer.StageTimes[stage].GetAverageTime(Ticks.Ticks20).TotalMilliseconds, 3)));
                Thread.Sleep(50);
            }
            while (!((Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Enter)));

            Console.CursorVisible = true;
            LogManager.Instance.EnableConsoleOutput();
        }

        #endregion
    }
}
