using log4net.Core;
using log4net.Repository.Hierarchy;
using MCBS;
using MCBS.Logging;
using Newtonsoft.Json;
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
        private static readonly LogImpl LOGGER = LogUtil.MainLogger;

        public static MinecraftConfig MinecraftConfig
        {
            get
            {
                if (_MinecraftConfig is null)
                    throw new InvalidOperationException();
                return _MinecraftConfig;
            }
        }
        private static MinecraftConfig? _MinecraftConfig;

        public static SystemConfig SystemConfig
        {
            get
            {
                if (_SystemConfig is null)
                    throw new InvalidOperationException();
                return _SystemConfig;
            }
        }
        private static SystemConfig? _SystemConfig;

        public static ScreenConfig ScreenConfig
        {
            get
            {
                if (_ScreenConfig is null)
                    throw new InvalidOperationException();
                return _ScreenConfig;
            }
        }
        private static ScreenConfig? _ScreenConfig;

        public static IReadOnlyDictionary<string, string> Registry
        {
            get
            {
                if (_Registry is null)
                    throw new InvalidOperationException();
                return _Registry;
            }
        }
        private static Dictionary<string, string>? _Registry;

        public static void CreateIfNotExists()
        {
            SR.McbsDirectory.Configs.CreateIfNotExists();
            CreateIfNotExists(SR.McbsDirectory.Configs.Log4Net, "MCBS.Config.Default.log4net.xml");
            CreateIfNotExists(SR.McbsDirectory.Configs.Minecraft, "MCBS.Config.Default.Minecraft.toml");
            CreateIfNotExists(SR.McbsDirectory.Configs.System, "MCBS.Config.Default.System.toml");
            CreateIfNotExists(SR.McbsDirectory.Configs.Screen, "MCBS.Config.Default.Screen.toml");
            CreateIfNotExists(SR.McbsDirectory.Configs.Registry, "MCBS.Config.Default.Registry.json");
        }

        private static void CreateIfNotExists(string path, string resource)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException($"“{nameof(path)}”不能为 null 或空。", nameof(path));
            if (string.IsNullOrEmpty(resource))
                throw new ArgumentException($"“{nameof(resource)}”不能为 null 或空。", nameof(resource));

            if (!File.Exists(path))
            {
                using Stream? stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource) ?? throw new InvalidOperationException();
                using FileStream fileStream = new(path, FileMode.Create);
                stream.CopyTo(fileStream);
                Console.WriteLine($"配置文件“{path}”不存在，已创建默认配置文件");
            }
        }

        public static void LoadAll()
        {
            _MinecraftConfig = MinecraftConfig.Load(SR.McbsDirectory.Configs.Minecraft);
            _SystemConfig = SystemConfig.Load(SR.McbsDirectory.Configs.System);
            _ScreenConfig = ScreenConfig.Load(SR.McbsDirectory.Configs.Screen);
            _Registry = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(SR.McbsDirectory.Configs.Registry)) ?? throw new FormatException();

            LOGGER.Info("配置文件加载完成");
        }
    }
}
