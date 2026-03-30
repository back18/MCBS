using MCBS.Config;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.Services
{
    public interface IMinecraftConfigProvider
    {
        public MinecraftConfig Config { get; }
    }
}
