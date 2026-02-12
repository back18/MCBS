using MCBS.Common.Logging;
using MCBS.Common.Services;
using MCBS.ConsoleTerminal.Services;
using MCBS.Drawing;
using MCBS.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuanLib.Commands;
using QuanLib.Downloader.Services;
using QuanLib.IO.Extensions;
using QuanLib.Logging;
using QuanLib.Minecraft.Downloading;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace MCBS.ConsoleTerminal
{
    public class ConsoleApp : IServiceProvider
    {
        private ConsoleApp()
        {
            ServiceCollection services = new();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        private readonly ServiceProvider _serviceProvider;

        public static bool IsLoaded { get; private set; }

        public static ConsoleApp Current
        {
            get => field ?? throw new InvalidOperationException("应用程序尚未初始化");
            private set => field = value;
        }

        public Terminal Terminal
        {
            get => GetNotNull(field);
            private set => field = value;
        }

        public CommandLogger CommandLogger
        {
            get => GetNotNull(field);
            private set => field = value;
        }

        public static ConsoleApp Create()
        {
            Current = new ConsoleApp();
            return Current;
        }

        public void Initialize()
        {
            if (IsLoaded)
                throw new InvalidOperationException("控制台完成已初始化，无法重复初始化");
            IsLoaded = true;

            Terminal = new AdvancedTerminal(new("SYSTEM", PrivilegeLevel.Root), _serviceProvider.GetRequiredService<QuanLib.Core.ILoggerProvider>());
            CommandLogger = new CommandLogger(McbsPathManager.MCBS_Logs.CombineFile("Command.log").FullName, loggerProvider: _serviceProvider.GetRequiredService<QuanLib.Core.ILoggerProvider>()) { IsWriteToFile = true };
            CommandLogger.AddBlacklist("time query gametime");
        }

        public void Start()
        {
            Terminal.Start("Terminal Thread");
            CommandLogger.Start("CommandLogger Thread");
        }

        public void Stop()
        {
            CommandLogger.Stop();
            CommandLogger.WaitForStop();
            Terminal.Stop();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(logging => logging.ClearProviders());

            services.AddSingleton(sp => Log4NetManager.Instance.GetProvider());
            services.AddSingleton<QuanLib.Core.ILoggerProvider>(sp => sp.GetRequiredService<ILog4NetProvider>());
            services.AddSingleton<ILoggerProvider>(sp => new MicrosoftLoggerProviderAdapter(sp.GetRequiredService<ILog4NetProvider>()));
            services.AddSingleton<ILoggerFactory, LoggerFactory>(sp =>
            {
                LoggerFactory loggerFactory = new();
                loggerFactory.AddProvider(sp.GetRequiredService<ILoggerProvider>());
                return loggerFactory;
            });

            services.AddSingleton<IMcbsPathProvider, McbsPathProvider>();
            services.AddSingleton<IConfigPathProvider, ConfigPathProvider>();
            services.AddSingleton<ICachePathProvider, CachePathProvider>();
            services.AddSingleton<ILogPathProvider, LogPathProvider>();
            services.AddSingleton<IMinecraftPathProvider, MinecraftPathProvider>();
            services.AddSingleton<IFFmpegPathProvider, FFmpegPathProvider>();

            services.AddSingleton<IMinecraftConfigProvider, MinecraftConfigProvider>();
            services.AddSingleton<ISystemConfigProvider, SystemConfigProvider>();
            services.AddSingleton<IScreenConfigProvider, ScreenConfigProvider>();
            services.AddSingleton<IRegistryConfigProvider, RegistryConfigProvider>();

            services.AddSingleton<IHashComputeService, HashComputeService>();
            services.AddSingleton<IAsyncHashComputeService, AsyncHashComputeService>();
            services.AddSingleton<IConfigResourceProvider, ConfigResourceProvider>();
            services.AddKeyedSingleton<IFileFactory>("MCBS.Common", (sp, _) => new ManifestResourceFileFactory(Assembly.Load("MCBS.Common"), "SystemResource"));
            services.AddSingleton<IJsonConfigLoadService, JsonConfigLoadService>();
            services.AddSingleton<IJsonConfigSaveService, JsonConfigSaveService>();
            services.AddKeyedSingleton<IConfigLoadService>("JSON", (sp, _) => sp.GetRequiredService<IJsonConfigLoadService>());
            services.AddKeyedSingleton<IConfigSaveService>("JSON", (sp, _) => sp.GetRequiredService<IJsonConfigSaveService>());
            services.AddSingleton<ITomlConfigLoadService, TomlConfigLoadService>();
            services.AddSingleton<ITomlConfigSaveService, TomlConfigSaveService>();
            services.AddKeyedSingleton<IConfigLoadService>("TOML", (sp, _) => sp.GetRequiredService<ITomlConfigLoadService>());
            services.AddKeyedSingleton<IConfigSaveService>("TOML", (sp, _) => sp.GetRequiredService<ITomlConfigSaveService>());

            services.AddSingleton<IDownloadService, DownloadService>();
            services.AddSingleton<IDownloadProgressFormatter, DownloadProgressFormatter>();
            services.AddKeyedSingleton<IMinecraftDownloadProvider, MojangDownloadProvider>("MOJANG");
            services.AddKeyedSingleton<IMinecraftDownloadProvider, BmclApiDownloadProvider>("BMCLAPI");
            services.AddKeyedSingleton<IFFmpegDownloadProvider, Win64FFmpegDownloadProvider>(nameof(OSPlatform.Windows));
            services.AddKeyedSingleton<IFFmpegDownloadProvider, Linux64FFmpegDownloadProvider>(nameof(OSPlatform.Linux));
            services.AddSingleton(sp =>
            {
                IMinecraftConfigProvider configProvider = sp.GetRequiredService<IMinecraftConfigProvider>();
                return sp.GetKeyedService<IMinecraftDownloadProvider>(configProvider.Config.DownloadSource) ??
                       sp.GetRequiredKeyedService<IMinecraftDownloadProvider>("MOJANG");
            });
            services.AddSingleton(sp =>
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    return sp.GetRequiredKeyedService<IFFmpegDownloadProvider>(nameof(OSPlatform.Windows));
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    return sp.GetRequiredKeyedService<IFFmpegDownloadProvider>(nameof(OSPlatform.Linux));
                else
                    throw new PlatformNotSupportedException("不支持的操作系统平台: " + Environment.OSVersion.VersionString);
            });

            services.AddSingleton<IColorToIndexConverter, ColorToIndexConverter>();
            services.AddSingleton<IColorMappingBuildService, ColorMappingBuildService>();
            services.AddSingleton<IColorMappingSerializer, ColorMappingSerializer>();
            services.AddSingleton<IColorMappingCacheLoader, ColorMappingCacheLoader>();
            services.AddKeyedSingleton<IColorMappingCacheFactory, ColorMappingFastCacheFactory>("FAST");
            services.AddKeyedSingleton<IColorMappingCacheFactory, ColorMappingCompCacheFactory>("COMP");
            services.AddSingleton(sp =>
            {
                ISystemConfigProvider configProvider = sp.GetRequiredService<ISystemConfigProvider>();
                string key = configProvider.Config.EnableCompressionCache ? "COMP" : "FAST";
                return sp.GetRequiredKeyedService<IColorMappingCacheFactory>(key);
            });

            services.AddKeyedSingleton<IMinecraftInstanceFactory, McapiMinecraftClientFactory>("CLIENT+MCAPI");
            services.AddKeyedSingleton<IMinecraftInstanceFactory, McapiMinecraftServerFactory>("SERVER+MCAPI");
            services.AddKeyedSingleton<IMinecraftInstanceFactory, RconMinecraftServerFactory>("SERVER+RCON");
            services.AddKeyedSingleton<IMinecraftInstanceFactory, ConsoleMinecraftServerFactory>("SERVER+CONSOLE");
            services.AddKeyedSingleton<IMinecraftInstanceFactory, HybridMinecraftServerFactory>("SERVER+HYBRID");
            services.AddSingleton(sp =>
            {
                IMinecraftConfigProvider configProvider = sp.GetRequiredService<IMinecraftConfigProvider>();
                string cs = configProvider.Config.IsServer ? "SERVER" : "CLIENT";
                string mode = configProvider.Config.CommunicationMode;
                string key = cs + '+' + mode;
                return sp.GetKeyedService<IMinecraftInstanceFactory>(key) ?? throw new NotSupportedException("不支持的Minecraft实例类型: " + key);
            });

            services.AddTransient<CoreConfigLoader>();
            services.AddTransient<TomlConfigLoader>();
            services.AddTransient<ILoggingLoader, Log4NetLoader>();
            services.AddTransient<CharacterWidthMappingLoader>();
            services.AddTransient<MinecraftResourceDownloader>();
            services.AddTransient<MinecraftResourceEntryLoader>();
            services.AddTransient<MinecraftResourceLoader>();
            services.AddKeyedTransient<IFFmpegLoader, Win64FFmpegLoader>(nameof(OSPlatform.Windows));
            services.AddKeyedTransient<IFFmpegLoader, Linux64FFmpegLoader>(nameof(OSPlatform.Linux));
            services.AddTransient(sp =>
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    return sp.GetRequiredKeyedService<IFFmpegLoader>(nameof(OSPlatform.Windows));
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    return sp.GetRequiredKeyedService<IFFmpegLoader>(nameof(OSPlatform.Linux));
                else
                    throw new PlatformNotSupportedException("不支持的操作系统平台: " + Environment.OSVersion.VersionString);
            });
        }

        public object? GetService(Type serviceType)
        {
            return _serviceProvider.GetService(serviceType);
        }

        private static T GetNotNull<T>(T? field, [CallerMemberName] string? propertyName = null) where T : class
        {
            return field ?? throw new InvalidOperationException($"属性“{propertyName}”未初始化");
        }
    }
}
