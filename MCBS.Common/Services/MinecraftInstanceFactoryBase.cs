using MCBS.Services;
using QuanLib.Core;
using QuanLib.Minecraft.Instance;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.Common.Services
{
    public abstract class MinecraftInstanceFactoryBase : IMinecraftInstanceFactory
    {
        protected MinecraftInstanceFactoryBase(ILoggerProvider loggerProvider, IMinecraftConfigProvider configProvider)
        {
            ArgumentNullException.ThrowIfNull(loggerProvider, nameof(loggerProvider));
            ArgumentNullException.ThrowIfNull(configProvider, nameof(configProvider));

            _loggerProvider = loggerProvider;
            _configProvider = configProvider;
        }

        protected readonly ILoggerProvider _loggerProvider;
        protected readonly IMinecraftConfigProvider _configProvider;

        public abstract MinecraftInstance CreateInstance();
    }
}
