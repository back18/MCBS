using CommunityToolkit.Mvvm.Messaging;
using MCBS.Common.Logging;
using MCBS.Common.Services;
using MCBS.Config;
using MCBS.Config.Minecraft;
using MCBS.Services;
using MCBS.WpfApp.Config;
using MCBS.WpfApp.Config.Extensions;
using MCBS.WpfApp.Helpers;
using MCBS.WpfApp.Messages;
using MCBS.WpfApp.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QuanLib.Downloader.Services;
using QuanLib.Logging;
using QuanLib.Minecraft.Downloading;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;

namespace MCBS.WpfApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application, IServiceProvider
    {
        private const string APPSETTINGS_PATH = "appsettings.json";
        private const string APPSETTINGS_PATH_FORMAT = "appsettings.{0}.json";
        private readonly IHost _host;

        public App()
        {
            InitializeComponent();

            _host = Host.CreateDefaultBuilder()
                .UseEnvironment(EnvironmentHelper.GetCurrentEnvironment())
                .ConfigureAppConfiguration(ConfigureAppConfiguration)
                .ConfigureLogging(ConfigureLogging)
                .ConfigureServices(ConfigureServices)
                .Build();
        }

        private void ConfigureAppConfiguration(HostBuilderContext context, IConfigurationBuilder config)
        {
            config.AddJsonFile(APPSETTINGS_PATH, optional: false, reloadOnChange: false);
            config.AddJsonFile(string.Format(APPSETTINGS_PATH_FORMAT, context.HostingEnvironment.EnvironmentName), optional: true, reloadOnChange: false);
            config.AddEnvironmentVariables();
        }

        private void ConfigureLogging(HostBuilderContext context, ILoggingBuilder logging)
        {
            ILog4NetProvider log4NetProvider = Log4NetBoot.Load();
            MicrosoftLoggerProviderAdapter loggerProvider = new(log4NetProvider);

            logging.ClearProviders();
            logging.AddProvider(loggerProvider);
        }

        private void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            services.AddSingleton(sp => Log4NetManager.Instance.GetProvider());
            services.AddSingleton<QuanLib.Core.ILoggerProvider>(sp => sp.GetRequiredService<ILog4NetProvider>());

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

            services.AddSingleton<ModernMessageBox>();
            services.AddSingleton<IMessageBoxService>(sp => sp.GetRequiredService<ModernMessageBox>());
            services.AddSingleton<IMessageBoxAsyncService>(sp => sp.GetRequiredService<ModernMessageBox>());
            services.AddSingleton<INavigable>(sp => sp.GetRequiredService<MainWindow>());
            services.AddSingleton<IConfigProvider, ConfigProvider>();
            services.AddKeyedSingleton(typeof(SystemConfig), (sp, key) => sp.GetRequiredService<IConfigProvider>().GetSystemConfigService());
            services.AddKeyedSingleton(typeof(MinecraftConfig), (sp, key) => sp.GetRequiredService<IConfigProvider>().GetMinecraftConfigService());
            services.AddKeyedSingleton(typeof(ScreenConfig), (sp, key) => sp.GetRequiredService<IConfigProvider>().GetScreenConfigService());
            services.AddKeyedSingleton(typeof(SystemConfig), (sp, key) => sp.GetRequiredKeyedService<IConfigStorage>(key).GetConfig());
            services.AddKeyedSingleton(typeof(MinecraftConfig), (sp, key) => sp.GetRequiredKeyedService<IConfigStorage>(key).GetConfig());
            services.AddKeyedSingleton(typeof(ScreenConfig), (sp, key) => sp.GetRequiredKeyedService<IConfigStorage>(key).GetConfig());

            services.AddKeyedSingleton(typeof(McapiModeConfig), (sp, key) =>
            {
                var service = sp.GetRequiredKeyedService<IConfigService>(typeof(MinecraftConfig));
                var model = ((MinecraftConfig.Model)service.GetCurrentConfig()).McapiModeConfig;
                return service.CreateSubservices(model);
            });
            services.AddKeyedSingleton(typeof(RconModeConfig), (sp, key) =>
            {
                var service = sp.GetRequiredKeyedService<IConfigService>(typeof(MinecraftConfig));
                var model = ((MinecraftConfig.Model)service.GetCurrentConfig()).RconModeConfig;
                return service.CreateSubservices(model);
            });
            services.AddKeyedSingleton(typeof(ConsoleModeConfig), (sp, key) =>
            {
                var service = sp.GetRequiredKeyedService<IConfigService>(typeof(MinecraftConfig));
                var model = ((MinecraftConfig.Model)service.GetCurrentConfig()).ConsoleModeConfig;
                return service.CreateSubservices(model);
            });

            services.AddSingleton<MainWindow>();

            services.AddTransient<IPageFactory, PageFactory>();
            services.AddTransient<IPageCreateFactory, PageCreateFactory>();
            services.AddTransient<CoreConfigLoader>();
            services.AddTransient<TomlConfigLoader>();

            services.Scan(scan => scan
                .FromAssemblyOf<App>()
                .AddClasses(cls => cls.InNamespaces("MCBS.WpfApp.ViewModels", "MCBS.WpfApp.Pages"))
                .AsSelf()
                .WithSingletonLifetime()
            );
        }

        [STAThread]
        private static void Main()
        {
            Thread.CurrentThread.Name = "Main Thread";

            App app = new();
            Initialize(app);

            app.MainWindow = app.GetRequiredService<MainWindow>();
            ThemeHelper.Initialize(app.MainWindow);
            BackdropHelper.Initialize(app.MainWindow);

            app.MainWindow.Show();
            app.Run();
        }

        private static void Initialize(IServiceProvider serviceProvider)
        {
            try
            {
                McbsPathManager.CreateAllDirectory();
                LanguageHelper.Initialize();

                CoreConfigLoader coreConfigLoader = serviceProvider.GetRequiredService<CoreConfigLoader>();
                coreConfigLoader.CreateIfNotExists();
                coreConfigLoader.LoadAll().ApplyInitialize();

                TomlConfigLoader tomlConfigLoader = serviceProvider.GetRequiredService<TomlConfigLoader>();
                tomlConfigLoader.CreateIfNotExists();
                tomlConfigLoader.LoadAll().ApplyInitialize();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("INIT FATAL: ");
                Console.Error.WriteLine(ex.ToString());
                throw;
            }
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            await _host.StartAsync();
            WeakReferenceMessenger.Default.Send(new AppStartupMessage(e));
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            WeakReferenceMessenger.Default.Send(new AppExitMessage(e));
            await _host.StopAsync();
            _host.Dispose();

            base.OnExit(e);
        }

        public object? GetService(Type serviceType)
        {
            return _host.Services.GetService(serviceType);
        }

        public static IServiceProvider GetServiceProvider()
        {
            return ((App)Current)._host.Services;
        }
    }
}
