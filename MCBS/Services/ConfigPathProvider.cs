using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.Services
{
    public class ConfigPathProvider : IConfigPathProvider
    {
        public FileInfo MinecraftConfig => McbsPathManager.MCBS_Config_MinecraftConfig;

        public FileInfo SystemConfig => McbsPathManager.MCBS_Config_SystemConfig;

        public FileInfo ScreenConfig => McbsPathManager.MCBS_Config_ScreenConfig;

        public FileInfo RegistryConfig => McbsPathManager.MCBS_Config_RegistryConfig;

        public FileInfo Log4NetConfig => McbsPathManager.MCBS_Config_Log4NetConfig;
    }
}
