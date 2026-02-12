using MCBS.Config.Minecraft;
using MCBS.Services;
using QuanLib.Core;
using QuanLib.Minecraft.Instance;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.Common.Services
{
    public class McapiMinecraftClientFactory(ILoggerProvider loggerProvider, IMinecraftConfigProvider configProvider) : MinecraftInstanceFactoryBase(loggerProvider, configProvider)
    {
        public override MinecraftInstance CreateInstance()
        {
            MinecraftConfig config = _configProvider.Config;
            return new McapiMinecraftClient(
                config.MinecraftPath,
                config.McapiModeConfig.Address,
                config.McapiModeConfig.Port,
                config.McapiModeConfig.Password,
                _loggerProvider);
        }
    }
}
