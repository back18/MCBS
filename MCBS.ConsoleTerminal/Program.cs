using log4net.Core;
using MCBS.Application;
using MCBS.BlockForms.Utility;
using MCBS.Config;
using MCBS.Config.Constants;
using MCBS.Config.Minecraft;
using QuanLib.Commands;
using QuanLib.Consoles;
using QuanLib.Core;
using QuanLib.IO.Extensions;
using QuanLib.Logging;
using QuanLib.Minecraft;
using QuanLib.Minecraft.Command.Events;
using QuanLib.Minecraft.Instance;
using QuanLib.Minecraft.ResourcePack;
using System.Diagnostics;
using System.Text;

namespace MCBS.ConsoleTerminal
{
    public static class Program
    {
        private static LogImpl LOGGER => LogManager.Instance.GetLogger();

        static Program()
        {
            Thread.CurrentThread.Name = "Main Thread";
            McbsPathManager.CreateAllDirectory();
            ConfigManager.CreateIfNotExists();
            LoadLogManager();
            LoadCharacterWidthMapping();
            Terminal = new AdvancedTerminal(new("SYSTEM", PrivilegeLevel.Root));
            CommandLogger = new(McbsPathManager.MCBS_Logs.CombineFile("Command.log").FullName) { IsWriteToFile = true };
            CommandLogger.AddBlacklist("time query gametime");
        }

        public static Terminal Terminal { get; }

        public static CommandLogger CommandLogger { get; }

        private static void Main(string[] args)
        {
            LOGGER.Info("MCBS已启动，欢迎使用！");

            Initialize();

            MinecraftConfig config = ConfigManager.MinecraftConfig;
            LOGGER.Info($"将以 {config.CommunicationMode} 模式绑定到位于“{config.MinecraftPath}”的Minecraft实例");

            MinecraftInstance minecraftInstance = CreateMinecraftInstance(config, LogManager.Instance.LoggerGetter);
            LOGGER.Info(GetMinecraftInstanceInfo(minecraftInstance));

            ApplicationManifest[] appComponents = AppComponentLoader.LoadAll();
            MinecraftBlockScreen mcbs = MinecraftBlockScreen.LoadInstance(new(minecraftInstance, appComponents));

            mcbs.Start("System Thread");
            Terminal.Start("Terminal Thread");
            CommandLogger.Start("CommandLogger Thread");

            minecraftInstance.CommandSender.CommandSent += CommandSender_CommandSent;

            mcbs.WaitForStop();

            Exit(0);
        }

        private static void Initialize()
        {
            try
            {
                LOGGER.Info("程序开始初始化");

                ConfigManager.LoadAll();
                using ResourceEntryManager resources = MinecraftResourcesLoader.LoadAll();
                SR.LoadAll(resources);
                FFmpegResourcesLoader.LoadAll();
                TextureManager.LoadInstance();

                LOGGER.Info("程序初始化完成");
            }
            catch (Exception ex)
            {
                LOGGER.Fatal("程序初始化失败", ex);
                Exit(-1);
                throw new InvalidOperationException();
            }
        }

        private static MinecraftInstance CreateMinecraftInstance(MinecraftConfig config, ILoggerGetter loggerGetter)
        {
            try
            {
                if (config.IsClient)
                {
                    if (config.CommunicationMode == CommunicationModes.MCAPI)
                        return new McapiMinecraftClient(
                            config.MinecraftPath,
                            config.McapiModeConfig.Address,
                            config.McapiModeConfig.Port,
                            config.McapiModeConfig.Password,
                            loggerGetter);
                    else
                        throw new InvalidOperationException();
                }
                else
                {
                    return config.CommunicationMode switch
                    {
                        CommunicationModes.MCAPI => new McapiMinecraftServer(
                            config.MinecraftPath,
                            config.ServerAddress,
                            config.ServerPort,
                            config.McapiModeConfig.Port,
                            config.McapiModeConfig.Password,
                            loggerGetter),
                        CommunicationModes.RCON => new RconMinecraftServer(config.MinecraftPath,
                            config.ServerAddress,
                            config.ServerPort,
                            config.RconModeConfig.Port,
                            config.RconModeConfig.Password,
                            loggerGetter),
                        CommunicationModes.CONSOLE => new ConsoleMinecraftServer(
                            config.MinecraftPath,
                            config.ServerAddress,
                            config.ServerPort,
                            new GenericServerLaunchArguments(
                                config.ConsoleModeConfig.JavaPath,
                                config.ConsoleModeConfig.LaunchArguments),
                            config.ConsoleModeConfig.MclogRegexFilter,
                            loggerGetter),
                        CommunicationModes.HYBRID => new HybridMinecraftServer(
                            config.MinecraftPath,
                            config.ServerAddress,
                            config.ServerPort,
                            config.RconModeConfig.Port,
                            config.RconModeConfig.Password,
                            new GenericServerLaunchArguments(
                                config.ConsoleModeConfig.JavaPath,
                                config.ConsoleModeConfig.LaunchArguments),
                            config.ConsoleModeConfig.MclogRegexFilter,
                            loggerGetter),
                        _ => throw new InvalidOperationException(),
                    };
                }
            }
            catch (Exception ex)
            {
                LOGGER.Fatal("无法创建Minecraft实例", ex);
                Exit(-1);
                throw new InvalidOperationException();
            }
        }

        public static string GetMinecraftInstanceInfo(MinecraftInstance minecraftInstance)
        {
            string farmat = "    - {0}：{1}" + Environment.NewLine;
            StringBuilder stringBuilder = new();

            stringBuilder.AppendLine("成功创建Minecraft实例");
            stringBuilder.AppendFormat(farmat, "通信模式", minecraftInstance.Identifier);
            stringBuilder.AppendFormat(farmat, "游戏路径", minecraftInstance.MinecraftPath);

            if (minecraftInstance is MinecraftServer minecraftServer)
            {
                stringBuilder.AppendFormat(farmat, "服务器地址", minecraftServer.ServerAddress);
                stringBuilder.AppendFormat(farmat, "服务器端口", minecraftServer.ServerPort);
            }

            if (minecraftInstance is IRconCapable rconCapable)
                stringBuilder.AppendFormat(farmat, "RCON端口", rconCapable.RconPort);

            if (minecraftInstance is IMcapiCapable mcapiCapable)
                stringBuilder.AppendFormat(farmat, "MCAPI端口", mcapiCapable.McapiPort);

            if (minecraftInstance is IConsoleCapable consoleCapable)
            {
                ProcessStartInfo startInfo = consoleCapable.ServerProcess.Process.StartInfo;
                stringBuilder.AppendFormat(farmat, "Java路径", startInfo.FileName);
                stringBuilder.AppendFormat(farmat, "启动参数", startInfo.Arguments);
            }

            stringBuilder.Length -= Environment.NewLine.Length;
            return stringBuilder.ToString();
        }

        private static void LoadLogManager()
        {
            using FileStream fileStream = File.OpenRead(McbsPathManager.MCBS_Configs_Log4NetConfig.FullName);
            LogManager.LoadInstance(new("[%date{HH:mm:ss}] [%t/%p] [%c]: %m%n", McbsPathManager.MCBS_Logs_LatestLog.FullName, Encoding.UTF8, fileStream, true));
        }

        private static void LoadCharacterWidthMapping()
        {
            FileInfo fileInfo = McbsPathManager.MCBS_Caches.CombineFile("CharacterWidthMapping.bin");

            if (fileInfo.Exists)
            {
                CharacterWidthMapping.LoadInstance(new(File.ReadAllBytes(fileInfo.FullName)));
            }
            else
            {
                CharacterWidthMapping characterWidthMapping = CharacterWidthMapping.LoadInstance(new(null));
                File.WriteAllBytes(fileInfo.FullName, characterWidthMapping.BuildCacheBytes());
            }
        }

        private static void CommandSender_CommandSent(QuanLib.Minecraft.Command.Senders.CommandSender sender, CommandInfoEventArgs e)
        {
            MinecraftBlockScreen mcbs = MinecraftBlockScreen.Instance;
            CommandLogger.Submit(new(e.CommandInfo, mcbs.GameTick, mcbs.SystemTick, mcbs.SystemStage));
        }

        public static void Exit(int exitCode)
        {
            CommandLogger.Stop();
            CommandLogger.WaitForStop();
            Terminal.Stop();

            for (int i = 10; i >= 1; i--)
            {
                LOGGER.Info($"MCBS将在{i}秒后退出，按下回车键立即退出");
                if (Console.KeyAvailable)
                {
                    if (Console.ReadKey(true).Key == ConsoleKey.Enter)
                        break;
                }
                else
                {
                    Thread.Sleep(1000);
                }
            }

            LOGGER.Info("MCBS已退出，感谢使用！");
            Environment.Exit(exitCode);
        }
    }
}
