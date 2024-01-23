using log4net.Core;
using log4net.Repository.Hierarchy;
using MCBS;
using Newtonsoft.Json;
using QuanLib.Logging;
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
            CreateIfNotExists(SR.McbsDirectory.ConfigsDir.Log4NetFile, SR.SystemResourceNamespace.ConfigsNamespace.Log4NetConfigFile);
            CreateIfNotExists(SR.McbsDirectory.ConfigsDir.MinecraftFile, SR.SystemResourceNamespace.ConfigsNamespace.MinecraftConfigFile);
            CreateIfNotExists(SR.McbsDirectory.ConfigsDir.SystemFile, SR.SystemResourceNamespace.ConfigsNamespace.SystemConfigFile);
            CreateIfNotExists(SR.McbsDirectory.ConfigsDir.ScreenFile, SR.SystemResourceNamespace.ConfigsNamespace.ScreenConfigFile);
            CreateIfNotExists(SR.McbsDirectory.ConfigsDir.RegistryFile, SR.SystemResourceNamespace.ConfigsNamespace.RegistryConfigFile);
        }

        private static void CreateIfNotExists(string path, string resource)
        {
            ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));
            ArgumentException.ThrowIfNullOrEmpty(resource, nameof(resource));

            if (!File.Exists(path))
            {
                using Stream? stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource) ?? throw new InvalidOperationException();
                FileStream fileStream = new(path, FileMode.Create);
                stream.CopyTo(fileStream);
                fileStream.Flush();
                fileStream.Close();
                LOGGER.Warn($"配置文件“{path}”不存在，已创建默认配置文件");
            }
        }

        public static void LoadAll()
        {
            _MinecraftConfig = MinecraftConfig.Load(SR.McbsDirectory.ConfigsDir.MinecraftFile);
            _SystemConfig = SystemConfig.Load(SR.McbsDirectory.ConfigsDir.SystemFile);
            _ScreenConfig = ScreenConfig.Load(SR.McbsDirectory.ConfigsDir.ScreenFile);
            _Registry = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(SR.McbsDirectory.ConfigsDir.RegistryFile)) ?? throw new FormatException();

            LOGGER.Info("配置文件加载完成");
        }
    }
}
