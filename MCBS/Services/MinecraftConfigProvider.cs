using MCBS.Config;
using MCBS.Config.Minecraft;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.Services
{
    public class MinecraftConfigProvider : IMinecraftConfigProvider
    {
        public MinecraftConfig Config => ConfigManager.MinecraftConfig;
    }
}
