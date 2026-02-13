using MCBS.Config.Minecraft;
using MCBS.Services;
using QuanLib.Core;
using QuanLib.Minecraft.Instance;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.Common.Services
{
    public class RconMinecraftServerFactory(ILoggerProvider loggerProvider, IMinecraftConfigProvider configProvider) : MinecraftInstanceFactoryBase(loggerProvider, configProvider)
    {
        public override MinecraftInstance CreateInstance()
        {
            MinecraftConfig config = _configProvider.Config;
            return new RconMinecraftServer(
                config.MinecraftPath,
                config.ServerAddress,
                config.ServerPort,
                config.RconModeConfig.Port,
                config.RconModeConfig.Password,
                _loggerProvider);
        }
    }
}
