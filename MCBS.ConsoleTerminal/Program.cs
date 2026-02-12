using MCBS.Application;
using MCBS.BlockForms.Utility;
using MCBS.Common.Services;
using MCBS.Config;
using MCBS.Config.Minecraft;
using MCBS.ConsoleTerminal.Services;
using Microsoft.Extensions.DependencyInjection;
using QuanLib.Consoles;
using QuanLib.Core;
using QuanLib.IO.Extensions;
using QuanLib.Logging;
using QuanLib.Minecraft.Command.Events;
using QuanLib.Minecraft.Instance;
using QuanLib.Minecraft.ResourcePack;
using System.Diagnostics;
using System.Text;

namespace MCBS.ConsoleTerminal
{
    public static class Program
    {
        private static ILogger LOGGER => Log4NetManager.Instance.GetLogger(typeof(Program));

        private static void Main(string[] args)
        {
            Thread.CurrentThread.Name = "Main Thread";

            Initialize();

            LOGGER.Info("MCBS已启动，欢迎使用！");

            LoadResourceAsync().GetAwaiter().GetResult();

            MinecraftConfig config = ConfigManager.MinecraftConfig;
            LOGGER.Info($"将以 {config.CommunicationMode} 模式绑定到位于“{config.MinecraftPath}”的Minecraft{(config.IsServer ? "服务端" : "客户端")}");

            MinecraftInstance minecraftInstance = CreateMinecraftInstance();
            LOGGER.Info(GetMinecraftInstanceInfo(minecraftInstance));

            LOGGER.Info($"正在等待位于“{config.MinecraftPath}”的Minecraft实例启动...");

            if (ConnectToMinecraft(minecraftInstance))
            {
                LOGGER.Info("成功连接到Minecraft实例");
            }
            else
            {
                LOGGER.Fatal("由于Minecraft实例启动失败，程序即将终止");
                Exit(-1);
                return;
            }

            minecraftInstance.CommandSender.CommandSent += CommandSender_CommandSent;
            minecraftInstance.Stopped += MinecraftInstance_Stopped;

            ApplicationManifest[] appComponents = AppComponentLoader.LoadAll();
            MinecraftBlockScreen mcbs = MinecraftBlockScreen.LoadInstance(new(minecraftInstance, appComponents));

            mcbs.Start("System Thread");
            ConsoleApp.Current.Start();

            mcbs.WaitForStop();
            mcbs.Dispose();

            minecraftInstance.Stop();
            minecraftInstance.Dispose();

            Exit(0);
        }

        private static void Initialize()
        {
            try
            {
                McbsPathManager.CreateAllDirectory();
                ConsoleApp consoleApp = ConsoleApp.Create();

                CoreConfigLoader coreConfigLoader = consoleApp.GetRequiredService<CoreConfigLoader>();
                coreConfigLoader.CreateIfNotExists();
                coreConfigLoader.LoadAll().ApplyInitialize();

                TomlConfigLoader tomlConfigLoader = consoleApp.GetRequiredService<TomlConfigLoader>();
                tomlConfigLoader.CreateIfNotExists();
                tomlConfigLoader.LoadAll().ApplyInitialize();

                ILoggingLoader loggingLoader = consoleApp.GetRequiredService<ILoggingLoader>();
                loggingLoader.Load();

                consoleApp.Initialize();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("FATAL: ");
                Console.Error.WriteLine(ex.ToString());
                Environment.Exit(-1);
            }
        }

        private static async Task LoadResourceAsync()
        {
            try
            {
                LOGGER.Info("开始加载外部资源");

                LoadCharacterWidthMapping();

                ConsoleApp consoleApp = ConsoleApp.Current;

                MinecraftResourceDownloader minecraftResourceDownloader = consoleApp.GetRequiredService<MinecraftResourceDownloader>();
                MinecraftResourceEntryLoader minecraftResourceEntryLoader = consoleApp.GetRequiredService<MinecraftResourceEntryLoader>();
                MinecraftResourceLoader minecraftResourceLoader = consoleApp.GetRequiredService<MinecraftResourceLoader>();

                await minecraftResourceDownloader.StartAsync();
                using ResourceEntryManager resources = await minecraftResourceEntryLoader.LoadAsync();
                (await minecraftResourceLoader.LoadAsync(resources)).ApplyInitialize();

                IFFmpegLoader ffmpegLoader = consoleApp.GetRequiredService<IFFmpegLoader>();
                await ffmpegLoader.LoadAsync();

                SR.LoadAll();
                TextureManager.LoadInstance();

                LOGGER.Info("所有资源已全部加载完成");
            }
            catch (Exception ex)
            {
                LOGGER.Fatal("资源加载失败", ex);
                Exit(-1);
            }
        }

        private static MinecraftInstance CreateMinecraftInstance()
        {
            try
            {
                IMinecraftInstanceFactory factory = ConsoleApp.Current.GetRequiredService<IMinecraftInstanceFactory>();
                return factory.CreateInstance();
            }
            catch (Exception ex)
            {
                LOGGER.Fatal("Minecraft实例创建失败", ex);
                Exit(-1);
                throw;
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

        public static bool ConnectToMinecraft(MinecraftInstance minecraftInstance)
        {
            if (minecraftInstance.IsSubprocess)
            {
                minecraftInstance.Start("MinecraftInstance Thread");
                minecraftInstance.WaitForConnection();
            }
            else
            {
                minecraftInstance.WaitForConnection();
                minecraftInstance.Start("MinecraftInstance Thread");
            }

            Thread.Sleep(1000);

            return minecraftInstance.IsRunning;
        }

        private static void LoadCharacterWidthMapping()
        {
            FileInfo fileInfo = McbsPathManager.MCBS_Cache.CombineFile("CharacterWidthMapping.bin");

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
            if (!MinecraftBlockScreen.IsInstanceLoaded)
                return;

            MinecraftBlockScreen mcbs = MinecraftBlockScreen.Instance;
            ConsoleApp.Current.CommandLogger.Submit(new(e.CommandInfo, mcbs.GameTick, mcbs.SystemTick, mcbs.SystemStage));
        }

        private static void MinecraftInstance_Stopped(IRunnable sender, EventArgs e)
        {
            if (!MinecraftBlockScreen.IsInstanceLoaded)
                return;

            MinecraftBlockScreen mcbs = MinecraftBlockScreen.Instance;
            if (!mcbs.IsRunning)
                return;

            LOGGER.Error("Minecraft实例意外断开连接，系统即将终止");
            mcbs.RequestStop();
        }

        public static void Exit(int exitCode)
        {
            ConsoleApp.Current.Stop();

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
