using FFMediaToolkit;
using log4net.Core;
using MCBS.Application;
using MCBS.BlockForms.Utility;
using MCBS.Config;
using MCBS.Logging;
using MCBS.Rendering;
using MCBS.SystemApplications;
using QuanLib.Minecraft;
using QuanLib.Minecraft.API;
using QuanLib.Minecraft.API.Instance;
using QuanLib.Minecraft.API.Packet;
using QuanLib.Minecraft.Command.Senders;
using QuanLib.Minecraft.Events;
using QuanLib.Minecraft.Instance;
using QuanLib.Minecraft.ResourcePack;
using QuanLib.Minecraft.ResourcePack.Language;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Text;

namespace MCBS.ConsoleTerminal
{
    public static class Program
    {
        private static readonly LogImpl LOGGER;

        static Program()
        {
            Thread.CurrentThread.Name = "Main Thread";
            ConfigManager.CreateIfNotExists();
            LOGGER = LogUtil.GetLogger();
            Terminal = new();
            CommandLogger = new();
            CommandLogger.IsWriteToFile = true;
        }

        public static Terminal Terminal { get; }

        public static CommandLogger CommandLogger { get; }

        private static void Main(string[] args)
        {
            LOGGER.Info("MCBS已启动，欢迎使用！");
            Terminal.Start("Terminal Thread");
            CommandLogger.Start("CommandLogger Thread");

            try
            {
                ConfigManager.LoadAll();
                FFmpegResourcesLoader.LoadAll();
                ResourceEntryManager resources = MinecraftResourcesLoader.LoadAll();
                SR.LoadAll(resources);
                TextureManager.LoadInstance();

                resources.Dispose();
                LOGGER.Info("资源包内所有资源均已加载完成，资源包缓存已释放");
            }
            catch (Exception ex)
            {
                LOGGER.Fatal("无法完成资源文件的加载", ex);
                Exit();
                return;
            }

            MinecraftInstance minecraftInstance;

            try
            {
                MinecraftConfig config = ConfigManager.MinecraftConfig;
                switch (config.InstanceType)
                {
                    case InstanceTypes.CLIENT:
                        if (config.CommunicationMode == CommunicationModes.MCAPI)
                            minecraftInstance = new McapiMinecraftClient(config.MinecraftPath, config.ServerAddress, config.McapiPort, config.McapiPassword, Logbuilder.Default);
                        else
                            throw new InvalidOperationException();
                        break;
                    case InstanceTypes.SERVER:
                        minecraftInstance = config.CommunicationMode switch
                        {
                            CommunicationModes.RCON => new RconMinecraftServer(config.MinecraftPath, config.ServerAddress, Logbuilder.Default),
                            CommunicationModes.CONSOLE => new ConsoleMinecraftServer(config.MinecraftPath, config.ServerAddress, new GenericServerLaunchArguments(config.JavaPath, config.LaunchArguments), Logbuilder.Default),
                            CommunicationModes.HYBRID => new HybridMinecraftServer(config.MinecraftPath, config.ServerAddress, new GenericServerLaunchArguments(config.JavaPath, config.LaunchArguments), Logbuilder.Default),
                            CommunicationModes.MCAPI => new McapiMinecraftServer(config.MinecraftPath, config.ServerAddress, config.McapiPort, config.McapiPassword, Logbuilder.Default),
                            _ => throw new InvalidOperationException(),
                        };
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }
            catch (Exception ex)
            {
                LOGGER.Fatal("无法绑定到Minecraft实例", ex);
                Exit();
                return;
            }

            ApplicationManifest[] appComponents = AppComponentLoader.LoadAll();
            MinecraftBlockScreen.LoadInstance(minecraftInstance, appComponents);
            MinecraftBlockScreen.Instance.MinecraftInstance.CommandSender.CommandSent += CommandSender_CommandSent;
            MinecraftBlockScreen.Instance.Start("System Thread");
            MinecraftBlockScreen.Instance.WaitForStop();

            Exit();
            return;
        }

        private static void CommandSender_CommandSent(CommandSender sender, CommandInfoEventArgs e)
        {
            //CommandLogger.Submit(new(e.CommandInfo, MCOS.Instance.GameTick, MCOS.Instance.SystemTick, MCOS.Instance.SystemStage, Thread.CurrentThread.Name ?? "null"));
        }

        public static void Exit()
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
        }
    }
}
