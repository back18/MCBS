using QuanLib.Minecraft.Instance;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.Common.Services
{
    public interface IMinecraftInstanceFactory
    {
        public MinecraftInstance CreateInstance();
    }
}
