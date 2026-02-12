using MCBS.Config.Minecraft;
using MCBS.Services;
using QuanLib.Core;
using QuanLib.Minecraft;
using QuanLib.Minecraft.Instance;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.Common.Services
{
    public class HybridMinecraftServerFactory(ILoggerProvider loggerProvider, IMinecraftConfigProvider configProvider) : MinecraftInstanceFactoryBase(loggerProvider, configProvider)
    {
        public override MinecraftInstance CreateInstance()
        {
            MinecraftConfig config = _configProvider.Config;
            return new HybridMinecraftServer(
                config.MinecraftPath,
                config.ServerAddress,
                config.ServerPort,
                config.RconModeConfig.Port,
                config.RconModeConfig.Password,
                new GenericServerLaunchArguments(
                    config.ConsoleModeConfig.JavaPath,
                    config.ConsoleModeConfig.LaunchArguments),
                config.ConsoleModeConfig.MclogRegexFilter,
                _loggerProvider);
        }
    }
}
