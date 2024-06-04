using log4net.Core;
using MCBS.Application;
using MCBS.BlockForms.Utility;
using MCBS.Config;
using MCBS.Config.Constants;
using MCBS.Config.Minecraft;
using QuanLib.Commands;
using QuanLib.Consoles;
using QuanLib.IO.Extensions;
using QuanLib.Logging;
using QuanLib.Minecraft;
using QuanLib.Minecraft.Command.Events;
using QuanLib.Minecraft.Command.Senders;
using QuanLib.Minecraft.Instance;
using QuanLib.Minecraft.ResourcePack;
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
            CommandLogger = new();
            CommandLogger.IsWriteToFile = true;
        }

        public static Terminal Terminal { get; }

        public static CommandLogger CommandLogger { get; }

        private static void Main(string[] args)
        {
            LOGGER.Info("MCBS已启动，欢迎使用！");

            Initialize();

            LOGGER.Info($"将以 {ConfigManager.MinecraftConfig.CommunicationMode} 模式绑定到位于“{ConfigManager.MinecraftConfig.MinecraftPath}”的Minecraft实例");

            MinecraftInstance minecraftInstance = BindingMinecraft();

            MinecraftConfig config = ConfigManager.MinecraftConfig;
            if (minecraftInstance is McapiMinecraftClient mcapiMinecraftClient)
                LOGGER.Info($"成功绑定到基于 {minecraftInstance.InstanceKey} 的Minecraft客户端，游戏路径:{minecraftInstance.MinecraftPath} 地址:{mcapiMinecraftClient.McapiAddress} 端口:{mcapiMinecraftClient.McapiPort}");
            else if (minecraftInstance is McapiMinecraftServer mcapiMinecraftServer)
                LOGGER.Info($"成功绑定到基于 {minecraftInstance.InstanceKey} 的Minecraft服务端，游戏路径:{minecraftInstance.MinecraftPath} 地址:{mcapiMinecraftServer.ServerAddress} 端口:{mcapiMinecraftServer.McapiPort}");
            else if (minecraftInstance is RconMinecraftServer rconMinecraftServer)
                LOGGER.Info($"成功绑定到基于 {minecraftInstance.InstanceKey} 的Minecraft服务端，游戏路径:{minecraftInstance.MinecraftPath} 地址:{rconMinecraftServer.ServerAddress} 端口:{rconMinecraftServer.RconPort}");
            else if (minecraftInstance is ConsoleMinecraftServer consoleMinecraftServer)
                LOGGER.Info($"成功绑定到基于 {minecraftInstance.InstanceKey} 的Minecraft服务端，游戏路径:{minecraftInstance.MinecraftPath} Java路径:{consoleMinecraftServer.ServerProcess.LaunchArguments.JavaPath} 启动参数:{consoleMinecraftServer.ServerProcess.LaunchArguments.GetArguments()}");
            else if (minecraftInstance is HybridMinecraftServer hybridMinecraftServer)
                LOGGER.Info($"成功绑定到基于 {minecraftInstance.InstanceKey} 的Minecraft服务端，游戏路径:{minecraftInstance.MinecraftPath} 地址:{hybridMinecraftServer.ServerAddress} 端口:{hybridMinecraftServer.RconPort} Java路径:{hybridMinecraftServer.ServerProcess.LaunchArguments.JavaPath} 启动参数:{hybridMinecraftServer.ServerProcess.LaunchArguments.GetArguments()}");
            else
                throw new InvalidOperationException();

            ApplicationManifest[] appComponents = AppComponentLoader.LoadAll();
            MinecraftBlockScreen.LoadInstance(new(minecraftInstance, appComponents));

            MinecraftBlockScreen.Instance.Start("System Thread");
            Terminal.Start("Terminal Thread");
            CommandLogger.Start("CommandLogger Thread");

            MinecraftBlockScreen.Instance.WaitForStop();

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

        private static MinecraftInstance BindingMinecraft()
        {
            try
            {
                MinecraftConfig config = ConfigManager.MinecraftConfig;
                switch (config.MinecraftType)
                {
                    case MinecraftTypes.CLIENT:
                        if (config.CommunicationMode == CommunicationModes.MCAPI)
                            return new McapiMinecraftClient(config.MinecraftPath, config.McapiModeConfig.Address, config.McapiModeConfig.Port, config.McapiModeConfig.Password, LogManager.Instance.LoggerGetter);
                        else
                            throw new InvalidOperationException();
                    case MinecraftTypes.SERVER:
                        return config.CommunicationMode switch
                        {
                            CommunicationModes.MCAPI => new McapiMinecraftServer(config.MinecraftPath, config.ServerAddress, config.ServerPort, config.McapiModeConfig.Port, config.McapiModeConfig.Password, LogManager.Instance.LoggerGetter),
                            CommunicationModes.RCON => new RconMinecraftServer(config.MinecraftPath, config.ServerAddress, config.ServerPort, config.RconModeConfig.Port, config.RconModeConfig.Password, LogManager.Instance.LoggerGetter),
                            CommunicationModes.CONSOLE => new ConsoleMinecraftServer(config.MinecraftPath, config.ServerAddress, config.ServerPort, new GenericServerLaunchArguments(config.ConsoleModeConfig.JavaPath, config.ConsoleModeConfig.LaunchArguments), LogManager.Instance.LoggerGetter),
                            CommunicationModes.HYBRID => new HybridMinecraftServer(config.MinecraftPath, config.ServerAddress, config.ServerPort, config.RconModeConfig.Port, config.RconModeConfig.Password, new GenericServerLaunchArguments(config.ConsoleModeConfig.JavaPath, config.ConsoleModeConfig.LaunchArguments), LogManager.Instance.LoggerGetter),
                            _ => throw new InvalidOperationException(),
                        };
                    default:
                        throw new InvalidOperationException();
                }
            }
            catch (Exception ex)
            {
                LOGGER.Fatal("无法绑定到Minecraft实例", ex);
                Exit(-1);
                throw new InvalidOperationException();
            }
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

        public static void Exit(int exitCode)
        {
            Task terminal = Terminal.WaitForStopAsync();
            Task commandLogger = CommandLogger.WaitForStopAsync();
            Terminal.Stop();
            CommandLogger.Stop();
            Task.WaitAll(terminal, commandLogger);

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
