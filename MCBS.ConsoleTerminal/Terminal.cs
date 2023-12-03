﻿using log4net.Core;
using MCBS.Logging;
using QuanLib;
using QuanLib.CommandLine;
using QuanLib.CommandLine.ConsoleTerminal;
using QuanLib.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Level = QuanLib.CommandLine.Level;

namespace MCBS.ConsoleTerminal
{
    public class Terminal : RunnableBase
    {
        private static readonly LogImpl LOGGER = LogUtil.GetLogger();

        public Terminal() : base(Logbuilder.Default)
        {
            CommandSystem = new(new(Level.Root));
            RegistrationCommands();
        }

        public CommandSystem CommandSystem { get; }

        protected override void Run()
        {
            LOGGER.Info("命令行终端已启动");

            while (IsRunning)
            {
                string? input = Console.ReadLine();
                if (!IsRunning)
                {
                    break;
                }
                if (!MCOS.IsLoaded)
                {
                    Console.WriteLine("【MCBS控制台】系统未加载，控制台输入已被禁用");
                    continue;
                }
                if (input is null)
                {
                    continue;
                }

                switch (input)
                {
                    case "help":
                        Console.WriteLine("【MCBS控制台】mcconsole--------Minecraft控制台");
                        Console.WriteLine("【MCBS控制台】mccommand--------Minecraft命令日志记录器");
                        Console.WriteLine("【MCBS控制台】commandsystem----可视化命令系统");
                        Console.WriteLine("【MCBS控制台】mspt------------MSPT实时计时器");
                        Console.WriteLine("【MCBS控制台】stop-------------终止系统并退出程序");
                        break;
                    case "mcconsole":
                        Console.WriteLine("【MCBS控制台】已进入Minecraft控制台");
                        Console.WriteLine("【MCBS控制台】该功能不可用");
                        Console.WriteLine("【MCBS控制台】已退出Minecraft控制台");
                        break;
                    case "mccommand":
                        Console.WriteLine("【MCBS控制台】已进入Minecraft命令日志记录器");
                        LogUtil.DisableConsoleOutput();
                        Program.CommandLogger.IsWriteToConsole = true;
                        WaitForInputEnter();
                        Program.CommandLogger.IsWriteToConsole = false;
                        LogUtil.EnableConsoleOutput();
                        Console.WriteLine("【MCBS控制台】已退出Minecraft命令日志记录器");
                        break;
                    case "commandsystem":
                        Console.WriteLine("【MCBS控制台】已进入可视化命令系统");
                        LogUtil.DisableConsoleOutput();
                        CommandSystem.Start();
                        LogUtil.EnableConsoleOutput();
                        Console.WriteLine("【MCBS控制台】已退出可视化命令系统");
                        break;
                    case "mspt":
                        Console.WriteLine("【MCBS控制台】已进入MSPT实时计时器");
                        LogUtil.DisableConsoleOutput();
                        Console.CursorVisible = false;
                        bool run = true;
                        Task.Run(() =>
                        {
                            string empty = new(' ', 32);
                            int lines = MCOS.Instance.TimeAnalysisManager.Count + 1;
                            for (int i = 0; i < lines; i++)
                                Console.WriteLine(empty);
                            while (run)
                            {
                                Console.CursorTop -= lines;
                                for (int i = 0; i < lines; i++)
                                    Console.WriteLine(empty);
                                Console.CursorTop -= lines;
                                Console.WriteLine(MCOS.Instance.TimeAnalysisManager.ToString());
                                Thread.Sleep(50);
                            }
                        });
                        WaitForInputEnter();
                        run = false;
                        Console.CursorVisible = true;
                        LogUtil.EnableConsoleOutput();
                        Console.WriteLine("【MCBS控制台】已退出MSPT实时计时器");
                        break;
                    case "stop":
                        if (!MCOS.IsLoaded || !MCOS.Instance.IsRunning)
                            Console.WriteLine("【MCBS控制台】系统未开始运行，因此无法关闭");
                        MCOS.Instance.Stop();
                        break;
                    default:
                        Console.WriteLine("【MCBS控制台】未知或不完整命令，输入“help”可查看可用命令列表");
                        break;
                }
            }

            LOGGER.Info("命令行终端已终止");
        }

        private static void WaitForInputEnter()
        {
            WaitForInputKey(ConsoleKey.Enter);
        }

        private static void WaitForInputKey(ConsoleKey key)
        {
            while (true)
            {
                if (Console.KeyAvailable)
                {
                    if (Console.ReadKey(true).Key == key)
                    {
                        break;
                    }
                }
                else
                {
                    Thread.Sleep(10);
                }
            }
        }

        private void RegistrationCommands()
        {
            CommandSystem.Pool.AddCommand(new(new("application list"), CommandFunc.GetFunc(GetApplicationList)));
            CommandSystem.Pool.AddCommand(new(new("screen list"), CommandFunc.GetFunc(GetScreenList)));
            CommandSystem.Pool.AddCommand(new(new("screen close"), CommandFunc.GetFunc(CloseScreen)));
            CommandSystem.Pool.AddCommand(new(new("screen builder"), CommandFunc.GetFunc(SetScreenBuilderEnable)));
            CommandSystem.Pool.AddCommand(new(new("process list"), CommandFunc.GetFunc(GetProcessList)));
            CommandSystem.Pool.AddCommand(new(new("form list"), CommandFunc.GetFunc(GetFormList)));
            CommandSystem.Pool.AddCommand(new(new("frame count"), CommandFunc.GetFunc(GetFrameCount)));
        }

        #region commands

        private static string GetApplicationList()
        {
            var list = MCOS.Instance.AppComponents;
            StringBuilder sb = new();
            sb.AppendLine($"当前已加载{list.Count}个应用程序，应用程序列表：");
            foreach (var applicationManifest in list.Values)
                sb.AppendLine(applicationManifest.ToString());

            return sb.ToString().TrimEnd();
        }

        private static string GetScreenList()
        {
            var list = MCOS.Instance.ScreenManager.Items;
            StringBuilder sb = new();
            sb.AppendLine($"当前已加载{list.Count}个屏幕，屏幕列表：");
            foreach (var context in list.Values)
                sb.AppendLine(context.ToString());

            return sb.ToString().TrimEnd();
        }

        private static string CloseScreen(int id)
        {
            if (MCOS.Instance.ScreenManager.Items.TryGetValue(id, out var context))
            {
                context.UnloadScreen();
                return $"屏幕“{context}”已关闭";
            }
            else
            {
                return "未知的屏幕ID";
            }
        }

        private static string SetScreenBuilderEnable(bool enable)
        {
            MCOS.Instance.ScreenBuildManager.Enable = enable;

            if (enable)
                return "屏幕构造器已启用";
            else
                return "屏幕构造器已禁用";
        }

        private static string GetProcessList()
        {
            var list = MCOS.Instance.ProcessManager.Items;
            StringBuilder sb = new();
            sb.AppendLine($"当前已启动{list.Count}个进程，进程列表：");
            foreach (var context in list.Values)
                sb.AppendLine(context.ToString());

            return sb.ToString().TrimEnd();
        }

        private static string GetFormList()
        {
            var list = MCOS.Instance.FormManager.Items;
            StringBuilder sb = new();
            sb.AppendLine($"当前已打开{list.Count}个窗体，窗体列表：");
            foreach (var context in list.Values)
                sb.AppendLine(context.ToString());

            return sb.ToString().TrimEnd();
        }

        private static int GetFrameCount()
        {
            return MCOS.Instance.SystemTick;
        }

        #endregion
    }
}
