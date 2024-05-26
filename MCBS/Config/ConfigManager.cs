using log4net.Core;
using log4net.Repository.Hierarchy;
using MCBS;
using MCBS.Config.Minecraft;
using Nett;
using Newtonsoft.Json;
using QuanLib.Core;
using QuanLib.Logging;
using QuanLib.TomlConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Config
{
    public static class ConfigManager
    {
        private static readonly LogImpl LOGGER = LogManager.Instance.GetLogger();

        public static MinecraftConfig MinecraftConfig => _MinecraftConfig ?? throw new InvalidOperationException("配置文件未加载");
        private static MinecraftConfig? _MinecraftConfig;

        public static SystemConfig SystemConfig => _SystemConfig ?? throw new InvalidOperationException("配置文件未加载");
        private static SystemConfig? _SystemConfig;

        public static ScreenConfig ScreenConfig => _ScreenConfig ?? throw new InvalidOperationException("配置文件未加载");
        private static ScreenConfig? _ScreenConfig;

        public static IReadOnlyDictionary<string, string> Registry => _Registry ?? throw new InvalidOperationException("配置文件未加载");
        private static Dictionary<string, string>? _Registry;

        public static void CreateIfNotExists()
        {
            CreateLog4NetConfig(McbsPathManager.MCBS_Configs_Log4NetConfig.FullName);
            CreateRegistryConfig(McbsPathManager.MCBS_Configs_RegistryConfig.FullName);
            CreateTomlConfig(McbsPathManager.MCBS_Configs_MinecraftConfig.FullName, MinecraftConfig.Model.CreateDefault());
            CreateTomlConfig(McbsPathManager.MCBS_Configs_SystemConfig.FullName, SystemConfig.Model.CreateDefault());
            CreateTomlConfig(McbsPathManager.MCBS_Configs_ScreenConfig.FullName, ScreenConfig.Model.CreateDefault());
        }

        public static void LoadAll()
        {
            _MinecraftConfig = MinecraftConfig.Load(McbsPathManager.MCBS_Configs_MinecraftConfig.FullName);
            _SystemConfig = SystemConfig.Load(McbsPathManager.MCBS_Configs_SystemConfig.FullName);
            _ScreenConfig = ScreenConfig.Load(McbsPathManager.MCBS_Configs_ScreenConfig.FullName);
            _Registry = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(McbsPathManager.MCBS_Configs_RegistryConfig.FullName)) ?? throw new FormatException();

            LOGGER.Info("配置文件加载完成");
        }

        private static void CreateLog4NetConfig(string path)
        {
            ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));

            if (File.Exists(path))
                return;

            MemoryStream memoryStream = LogManager.CreateDefaultXmlConfigStream();
            FileStream fileStream = new(path, FileMode.Create);
            memoryStream.CopyTo(fileStream);
            fileStream.Flush();
            fileStream.Close();
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

        private static void CreateTomlConfig(string path, object model)
        {
            ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));
            ArgumentNullException.ThrowIfNull(model, nameof(model));

            if (File.Exists(path))
                return;

            TomlTable tomlTable = TomlConfigBuilder.Build(model);
            string toml = tomlTable.ToString();
            File.WriteAllText(path, toml);
        }
    }
}
