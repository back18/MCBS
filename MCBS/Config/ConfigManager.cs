using MCBS.Config.Minecraft;
using Nett;
using Newtonsoft.Json;
using QuanLib.Core;
using QuanLib.Logging;
using QuanLib.TomlConfig;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Config
{
    public static class ConfigManager
    {
        private static readonly ILogger LOGGER = Log4NetManager.Instance.GetLogger();

        public static MinecraftConfig MinecraftConfig => _MinecraftConfig ?? throw new InvalidOperationException("配置文件未加载");
        private static MinecraftConfig? _MinecraftConfig;

        public static SystemConfig SystemConfig => _SystemConfig ?? throw new InvalidOperationException("配置文件未加载");
        private static SystemConfig? _SystemConfig;

        public static ScreenConfig ScreenConfig => _ScreenConfig ?? throw new InvalidOperationException("配置文件未加载");
        private static ScreenConfig? _ScreenConfig;

        public static ReadOnlyDictionary<string, string> Registry => _Registry ?? throw new InvalidOperationException("配置文件未加载");
        private static ReadOnlyDictionary<string, string>? _Registry;
        private static Dictionary<string, string>? _registry;

        public static void CreateIfNotExists()
        {
            CreateLog4NetConfig(McbsPathManager.MCBS_Config_Log4NetConfig.FullName);
            CreateRegistryConfig(McbsPathManager.MCBS_Config_RegistryConfig.FullName);
            CreateTomlConfig<MinecraftConfig.Model>(McbsPathManager.MCBS_Config_MinecraftConfig.FullName);
            CreateTomlConfig<SystemConfig.Model>(McbsPathManager.MCBS_Config_SystemConfig.FullName);
            CreateTomlConfig<ScreenConfig.Model>(McbsPathManager.MCBS_Config_ScreenConfig.FullName);
        }

        public static void LoadAll()
        {
            _MinecraftConfig = MinecraftConfig.Load(McbsPathManager.MCBS_Config_MinecraftConfig.FullName);
            _SystemConfig = SystemConfig.Load(McbsPathManager.MCBS_Config_SystemConfig.FullName);
            _ScreenConfig = ScreenConfig.Load(McbsPathManager.MCBS_Config_ScreenConfig.FullName);
            _Registry = LoadRegistry(McbsPathManager.MCBS_Config_RegistryConfig.FullName);

            LOGGER.Info("配置文件加载完成");
        }

        private static ReadOnlyDictionary<string, string> LoadRegistry(string path)
        {
            ThrowHelper.FileNotFound(path);

            string json = File.ReadAllText(path);
            _registry = JsonConvert.DeserializeObject<Dictionary<string, string>>(json) ?? throw new FormatException();

            return _registry.AsReadOnly();
        }

        private static void CreateLog4NetConfig(string path)
        {
            ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));

            if (File.Exists(path))
                return;

            Assembly assembly = Assembly.GetExecutingAssembly();
            using Stream stream = assembly.GetManifestResourceStream(assembly.GetName().Name + ".SystemResource.log4net.xml") ?? throw new InvalidOperationException();
            using FileStream fileStream = new(path, FileMode.Create);
            stream.CopyTo(fileStream);
            fileStream.Flush();
        }

        private static void CreateRegistryConfig(string path)
        {
            ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));

            if (File.Exists(path))
                return;

            Dictionary<string, string> dictionary = new()
            {
                { "txt", "System.Notepad" },
                { "log", "System.Notepad" },
                { "json", "System.Notepad" },
                { "xml", "System.Notepad" },
                { "jpg", "System.Album" },
                { "jpeg", "System.Album" },
                { "png", "System.Album" },
                { "bmp", "System.Album" },
                { "webp", "System.Album" },
                { "mp4", "System.VideoPlayer" },
                { "avi", "System.VideoPlayer" },
                { "wmv", "System.VideoPlayer" },
                { "mkv", "System.VideoPlayer" },
                { "process", "System.Console" }
            };

            string json = JsonConvert.SerializeObject(dictionary);
            File.WriteAllText(path, json);
        }

        private static void CreateTomlConfig<T>(string path) where T : IDataModel<T>
        {
            ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));

            if (File.Exists(path))
                return;

            TomlTable tomlTable = TomlConfigBuilder.Build(T.CreateDefault());
            string toml = tomlTable.ToString();
            File.WriteAllText(path, toml);
        }
    }
}
