using CommunityToolkit.Mvvm.Messaging;
using MCBS.Config;
using MCBS.Config.Minecraft;
using MCBS.WpfApp.Config;
using MCBS.WpfApp.Config.Extensions;
using MCBS.WpfApp.Logging;
using MCBS.WpfApp.Messages;
using MCBS.WpfApp.Pages;
using MCBS.WpfApp.Pages.Settings;
using MCBS.WpfApp.Services;
using MCBS.WpfApp.ViewModels.Settings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using QuanLib.Logging;
using System.IO;
using System.Windows;
using WPFLocalizeExtension.Engine;

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

            McbsPathManager.CreateAllDirectory();
            ConfigManager.CreateIfNotExists();
            LoadLogManager();

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
            logging.ClearProviders();
            logging.AddProvider(new MicrosoftLoggerProviderAdapter(Log4NetManager.Instance.GetProvider()));
        }

        private void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
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
            services.AddSingleton<HomePage>();
            services.AddSingleton<SimulatorPage>();
            services.AddSingleton<ManagerPage>();
            services.AddSingleton<DebuggerPage>();
            services.AddSingleton<SettingsPage>();

            services.AddSingleton<ApplicationSettingsPage>();
            services.AddSingleton<SystemSettingsPage>();
            services.AddSingleton<MinecraftSettingsPage>();
            services.AddSingleton<ScreenSettingsPage>();
            services.AddSingleton<McapiModeConfigPage>();
            services.AddSingleton<RconModeConfigPage>();
            services.AddSingleton<ConsoleModeConfigPage>();

            services.AddSingleton<ApplicationSettingsViewModel>();
            services.AddSingleton<SettingsViewModel>();
            services.AddSingleton<SystemSettingsViewModel>();
            services.AddSingleton<MinecraftSettingsViewModel>();
            services.AddSingleton<ScreenSettingsViewModel>();
            services.AddSingleton<McapiModeConfigViewModel>();
            services.AddSingleton<RconModeConfigViewModel>();
            services.AddSingleton<ConsoleModeConfigViewModel>();

            services.AddTransient<IPageFactory, PageFactory>();
            services.AddTransient<IPageCreateFactory, PageCreateFactory>();
        }

        [STAThread]
        private static void Main()
        {
            Thread.CurrentThread.Name = "Main Thread";
            LocalizeDictionary.Instance.Culture = Thread.CurrentThread.CurrentCulture;

            App app = new();
            app.MainWindow = app._host.Services.GetRequiredService<MainWindow>();
            app.MainWindow.Show();
            app.Run();
        }

        private static void LoadLogManager()
        {
            using FileStream fileStream = McbsPathManager.MCBS_Config_Log4NetConfig.OpenRead();
            Log4NetProvider provider = new(McbsPathManager.MCBS_Logs_LatestLog.FullName, fileStream, true);
            Log4NetManager.LoadInstance(new(provider));
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
