//#define TryCatch

using FFMediaToolkit;
using log4net.Core;
using MCBS.BlockForms.Utility;
using MCBS.Config;
using MCBS.Logging;
using MCBS.SystemApplications;
using QuanLib.Minecraft;
using QuanLib.Minecraft.API;
using QuanLib.Minecraft.API.Instance;
using QuanLib.Minecraft.API.Packet;
using QuanLib.Minecraft.Instance;
using QuanLib.Minecraft.ResourcePack.Language;
using System.Net;
using System.Text;

namespace MCBS.ConsoleTerminal
{
    public static class Program
    {
        private static readonly LogImpl LOGGER = LogUtil.MainLogger;

        private static void Main(string[] args)
        {
            Thread.CurrentThread.Name = "MainThread";
            ConfigManager.CreateIfNotExists();
            LOGGER.Info("Starting!");

            Terminal terminal = new();
            terminal.Start();

#if TryCatch
            try
            {
#endif
                ConfigManager.LoadAll();
                MinecraftResourcesManager.LoadAll();
                SR.LoadAll();
                TextureManager.LoadInstance();
                FFmpegResourcesLoader.LoadAll();
#if TryCatch
            }
            catch (Exception ex)
            {
                LOGGER.Fatal("无法完成资源文件的加载", ex);
                Exit();
                return;
            }
#endif

            MinecraftInstance minecraftInstance;
            MCOS mcos;

#if TryCatch
            try
            {
#endif
            MinecraftConfig config = ConfigManager.MinecraftConfig;
            switch (config.InstanceType)
            {
                case InstanceTypes.CLIENT:
                    if (config.CommunicationMode == CommunicationMods.MCAPI)
                        minecraftInstance = new McapiMinecraftClient(config.MinecraftPath, config.ServerAddress, config.McapiPort, config.McapiPassword);
                    else
                        throw new InvalidOperationException();
                    break;
                case InstanceTypes.SERVER:
                    minecraftInstance = config.CommunicationMode switch
                    {
                        CommunicationMods.RCON => new RconMinecraftServer(config.MinecraftPath, config.ServerAddress),
                        CommunicationMods.CONSOLE => new ConsoleMinecraftServer(config.MinecraftPath, config.ServerAddress, new GenericServerLaunchArguments(config.JavaPath, config.LaunchArguments)),
                        CommunicationMods.HYBRID => new HybridMinecraftServer(config.MinecraftPath, config.ServerAddress, new GenericServerLaunchArguments(config.JavaPath, config.LaunchArguments)),
                        CommunicationMods.MCAPI => new McapiMinecraftServer(config.MinecraftPath, config.ServerAddress, config.McapiPort, config.McapiPassword),
                        _ => throw new InvalidOperationException(),
                    };
                    break;
                default:
                    throw new InvalidOperationException();
            }

            minecraftInstance.Start();
            Thread.Sleep(1000);
#if TryCatch
            }
            catch (Exception ex)
            {
                LOGGER.Fatal("无法绑定到Minecraft实例", ex);
                Exit();
                return;
            }
#endif

            mcos = MCOS.LoadInstance(minecraftInstance);
            ApplicationLoader.LoadApplication(ref mcos, ConfigManager.SystemConfig.ExternalAppsFolder);

            mcos.Start();
            mcos.WaitForStop();

            Exit();
            return;

            void Exit()
            {
                terminal.Stop();
                LOGGER.Info("按下回车键退出...");
                terminal.WaitForStop();
            }
        }
    }
}
