using MCBS.Config;
using MCBS.Config.Minecraft;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.WpfApp.Config
{
    public class ConfigProvider : IConfigProvider
    {
        public IConfigStorage GetSystemConfigService()
        {
            return new TomlConfigFileStorage(new ConfigDataModel<SystemConfig.Model>(), McbsPathManager.MCBS_Config_SystemConfig.FullName, Encoding.UTF8);
        }

        public IConfigStorage GetMinecraftConfigService()
        {
            return new TomlConfigFileStorage(new ConfigDataModel<MinecraftConfig.Model>(), McbsPathManager.MCBS_Config_MinecraftConfig.FullName, Encoding.UTF8);
        }

        public IConfigStorage GetScreenConfigService()
        {
            return new TomlConfigFileStorage(new ConfigDataModel<ScreenConfig.Model>(), McbsPathManager.MCBS_Config_ScreenConfig.FullName, Encoding.UTF8);
        }
    }
}
