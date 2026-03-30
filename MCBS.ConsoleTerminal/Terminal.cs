using MCBS.Analyzer;
using MCBS.ConsoleTerminal.Extensions;
using MCBS.Forms;
using MCBS.Processes;
using MCBS.Screens;
using QuanLib.Commands;
using QuanLib.Commands.CommandLine;
using QuanLib.Commands.Syntaxes;
using QuanLib.Consoles;
using QuanLib.Core;
using QuanLib.Core.Events;
using QuanLib.IO.Extensions;
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
        protected Terminal(CommandSender commandSender, ILoggerProvider? loggerProvider = null) : base(loggerProvider)
        {
            ArgumentNullException.ThrowIfNull(commandSender, nameof(commandSender));

            _commandSender = commandSender;
            CommandManager = new();
            RegisterCommands();
        }

        protected readonly CommandSender _commandSender;

        public CommandManager CommandManager { get; }

        protected abstract CommandReadResult ReadCommand();

        protected override void Run()
        {
            while (IsRunning)
            {
                if (!MinecraftBlockScreen.IsLoaded || !MinecraftBlockScreen.Instance.IsRunning)
                {
                    break;
                }

                CommandReadResult result = ReadCommand();

                if (!IsRunning || result == CommandReadResult.Empty)
                {
                    break;
                }

                if (!MinecraftBlockScreen.IsLoaded || !MinecraftBlockScreen.Instance.IsRunning)
                {
                    Console.WriteLine("MCBS未在运行，无法执行命令");
                    continue;
                }

                ICommand? command = result.Command;
                SyntaxTree? syntaxTree = result.SyntaxTree;

                if (command is null)
                {
                    Console.WriteLine("未知或不完整命令");
                    continue;
                }

                try
                {
                    string[]? arguments = syntaxTree?.BuildArguments();
                    string message = command.Execute(_commandSender, arguments);

                    if (Console.CursorLeft > 0)
                        Console.WriteLine();

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
                CommandBuilder.Create()
                .On("help")
                .Allow(PrivilegeLevel.User)
                .Execute(Help)
                .Build());

            CommandManager.Register(
                CommandBuilder.Create()
                .On("application list")
                .Allow(PrivilegeLevel.User)
                .Execute(GetApplicationList)
                .Build());

            CommandManager.Register(
                CommandBuilder.Create()
                .On("screen list")
                .Allow(PrivilegeLevel.User)
                .Execute(GetScreenList)
                .Build());

            CommandManager.Register(
                CommandBuilder.Create()
                .On("process list")
                .Allow(PrivilegeLevel.User)
                .Execute(GetProcessList)
                .Build());

            CommandManager.Register(
                CommandBuilder.Create()
                .On("form list")
                .Allow(PrivilegeLevel.User)
                .Execute(GetFormList)
                .Build());

            CommandManager.Register(
                CommandBuilder.Create()
                .On("screen forms")
                .Allow(PrivilegeLevel.User)
                .Execute(GetFormsOfScreen)
                .Build());

            CommandManager.Register(
                CommandBuilder.Create()
                .On("screen builder enable")
                .Allow(PrivilegeLevel.User)
                .Execute(EnableScreenBuilder)
                .Build());

            CommandManager.Register(
                CommandBuilder.Create()
                .On("screen builder disable")
                .Allow(PrivilegeLevel.User)
                .Execute(DisableScreenBuilder)
                .Build());

            CommandManager.Register(
                CommandBuilder.Create()
                .On("mcbs SystemTick")
                .Allow(PrivilegeLevel.User)
                .Execute(GetSystemTick)
                .Build());

            CommandManager.Register(
                CommandBuilder.Create()
                .On("mcbs stop")
                .Allow(PrivilegeLevel.User)
                .Execute(StopMCBS)
                .Build());

            CommandManager.Register(
                CommandBuilder.Create()
                .On("mspt analyzer")
                .Allow(PrivilegeLevel.User)
                .Execute(MsptAnalysis)
                .Build());

            CommandManager.Register(
                CommandBuilder.Create()
                .On("mspt statistical")
                .Allow(PrivilegeLevel.User)
                .Execute((Action)MsptStatisticalChart)
                .Execute((Action<SystemStage>)MsptStatisticalChart)
                .Build());

            CommandManager.Register(
                CommandBuilder.Create()
                .On("commandlog domp")
                .Allow(PrivilegeLevel.User)
                .Execute((Func<string>)DumpCommandLog)
                .Execute((Func<int, string>)DumpCommandLog)
                .Build());
        }

        #region commands


        private string Help()
        {
            IEnumerable<string> identifiers = CommandManager.Keys.Order();
            StringBuilder stringBuilder = new();

            foreach (string identifier in identifiers)
            {
                ICommand command = CommandManager[identifier];
                string help = command.GetHelp();
                stringBuilder.AppendLine(help);
            }

            if (stringBuilder.Length > 0)
                stringBuilder.Length -= Environment.NewLine.Length;

            return stringBuilder.ToString();
        }

        private static string GetApplicationList()
        {
            MinecraftBlockScreen mcbs = MinecraftBlockScreen.Instance;
            StringBuilder stringBuilder = new();

            mcbs.SubmitAndWait(() =>
            {
                stringBuilder.AppendLine($"当前已加载{mcbs.AppComponents.Count}个应用程序，应用程序列表：");
                foreach (var applicationManifest in mcbs.AppComponents.Values)
                    stringBuilder.AppendLine(applicationManifest.ToString());
                stringBuilder.Length -= Environment.NewLine.Length;
            });

            return stringBuilder.ToString();
        }

        private static string GetScreenList()
        {
            MinecraftBlockScreen mcbs = MinecraftBlockScreen.Instance;
            StringBuilder stringBuilder = new();

            mcbs.SubmitAndWait(() =>
            {
                ScreenContext[] screens = mcbs.ScreenManager.Collection.GetScreens();
                stringBuilder.AppendLine($"当前已加载{screens.Length}个屏幕，屏幕列表：");
                foreach (ScreenContext screenContext in screens)
                    stringBuilder.AppendLine(screenContext.ToString());
                stringBuilder.Length -= Environment.NewLine.Length;
            });

            return stringBuilder.ToString();
        }

        private static string GetProcessList()
        {
            MinecraftBlockScreen mcbs = MinecraftBlockScreen.Instance;
            StringBuilder stringBuilder = new();

            mcbs.SubmitAndWait(() =>
            {
                ProcessContext[] processes = mcbs.ProcessManager.Collection.GetProcesses();
                stringBuilder.AppendLine($"当前已启动{processes.Length}个进程，进程列表：");
                foreach (ProcessContext processContext in processes)
                    stringBuilder.AppendLine(processContext.ToString());
                stringBuilder.Length -= Environment.NewLine.Length;
            });

            return stringBuilder.ToString();
        }

        private static string GetFormList()
        {
            MinecraftBlockScreen mcbs = MinecraftBlockScreen.Instance;
            StringBuilder stringBuilder = new();

            mcbs.SubmitAndWait(() =>
            {
                FormContext[] forms = mcbs.FormManager.Collection.GetForms();
                stringBuilder.AppendLine($"当前已打开{forms.Length}个窗体，窗体列表：");
                foreach (FormContext formContext in forms)
                    stringBuilder.AppendLine(formContext.ToString());
                stringBuilder.Length -= Environment.NewLine.Length;
            });

            return stringBuilder.ToString();
        }

        private static string GetFormsOfScreen(string screenId)
        {
            MinecraftBlockScreen mcbs = MinecraftBlockScreen.Instance;
            if (!mcbs.ScreenManager.Collection.TryGetScreen(screenId, out var screenContext))
                return $"找不到ID为 {screenId} 的屏幕";

            StringBuilder stringBuilder = new();
            mcbs.SubmitAndWait(() =>
            {
                FormContext[] forms = mcbs.FormManager.Collection.Where(s => s.RootForm == screenContext.RootForm).ToArray();
                stringBuilder.AppendLine($"屏幕({screenId})当前已打开{forms.Length}个窗体，窗体列表：");
                foreach (FormContext formContext in forms)
                    stringBuilder.AppendLine(formContext.ToString());
                stringBuilder.Length -= Environment.NewLine.Length;
            });

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

        private static void StopMCBS()
        {
            MinecraftBlockScreen.Instance.Stop();
        }

        private static void MsptAnalysis()
        {
            Log4NetManager.Instance.DisableConsoleOutput();
            Console.CursorVisible = false;

            ConsoleDynamicViwe consoleDynamicViwe = new(MinecraftBlockScreen.Instance.MsptAnalyzer.ToConsoleViwe, 50);
            consoleDynamicViwe.Start();
            ConsoleUtil.WaitForInputKey(ConsoleKey.Enter, 10);
            consoleDynamicViwe.Stop();

            Console.CursorVisible = true;
            Log4NetManager.Instance.ResumeConsoleOutput();
        }

        private static void MsptStatisticalChart()
        {
            MsptSlice msptSlice = MinecraftBlockScreen.Instance.MsptAnalyzer.TickTime;
            MsptStatisticalChart(msptSlice);
        }

        private static void MsptStatisticalChart(SystemStage stage)
        {
            int tick = MinecraftBlockScreen.Instance.SystemTick;
            MsptSlice msptSlice = MinecraftBlockScreen.Instance.MsptAnalyzer.StageTimes[stage];
            MsptStatisticalChart(msptSlice);
        }

        private static string DumpCommandLog()
        {
            FileInfo fileInfo = McbsPathManager.MCBS_Logs.CombineFile($"Command-{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.log");
            using FileStream fileStream = fileInfo.Create();
            ConsoleApp.Current.CommandLogger.Dump(fileStream);
            return $"命令日志已保存到“{fileInfo.FullName}”";
        }

        private static string DumpCommandLog(int maxLogCount)
        {
            FileInfo fileInfo = McbsPathManager.MCBS_Logs.CombineFile($"Command-{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.log");
            using FileStream fileStream = fileInfo.Create();
            ConsoleApp.Current.CommandLogger.Dump(fileStream, maxLogCount);
            return $"命令日志已保存到“{fileInfo.FullName}”";
        }

        #endregion

        private static void MsptStatisticalChart(MsptSlice msptSlice)
        {
            Log4NetManager.Instance.DisableConsoleOutput();
            Console.CursorVisible = false;

            msptSlice.TimeUpdated += WriteText;
            ConsoleUtil.WaitForInputKey(ConsoleKey.Enter, 10);
            msptSlice.TimeUpdated -= WriteText;

            Console.CursorVisible = true;
            Log4NetManager.Instance.ResumeConsoleOutput();

            void WriteText(MsptSlice sender, EventArgs<TimeSpan> e)
            {
                int width = Console.BufferWidth;
                double mspt = Math.Round(msptSlice.LatestTime.TotalMilliseconds, 2);
                int tick = MinecraftBlockScreen.Instance.SystemTick;

                string text = string.Format("|{0}ms/{1}t", mspt, tick);
                StringBuilder stringBuilder = new();

                int remaining = width - text.Length;
                if (remaining > 0)
                {
                    int full = Math.Min((int)mspt, remaining);
                    int empty = Math.Clamp(remaining - full, 0, Math.Max(0, 50 - full));

                    stringBuilder.Append('=', full);
                    stringBuilder.Append(' ', empty);
                }

                ConsoleColor color = Console.ForegroundColor;
                if (mspt > 40)
                    Console.ForegroundColor = ConsoleColor.Red;
                else if (mspt > 30)
                    Console.ForegroundColor = ConsoleColor.Yellow;
                else
                    Console.ForegroundColor = ConsoleColor.Green;

                Console.Write(stringBuilder);
                Console.ForegroundColor = color;
                Console.WriteLine(text);
            }
        }
    }
}
