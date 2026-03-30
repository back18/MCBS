using MCBS.Config;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.Services.Implementations
{
    public class MinecraftConfigProvider : IMinecraftConfigProvider
    {
        public MinecraftConfig Config => ConfigManager.MinecraftConfig;
    }
}
