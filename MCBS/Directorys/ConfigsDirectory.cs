using QuanLib.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Directorys
{
    public class ConfigsDirectory : DirectoryBase
    {
        public ConfigsDirectory(string directory) : base(directory)
        {
            MinecraftFile = Combine("Minecraft.toml");
            SystemFile = Combine("System.toml");
            ScreenFile = Combine("Screen.toml");
            RegistryFile = Combine("Registry.json");
            Log4NetFile = Combine("log4net.xml");
        }

        public string MinecraftFile { get; }

        public string SystemFile { get; }

        public string ScreenFile { get; }

        public string RegistryFile { get; }

        public string Log4NetFile { get; }
    }
}
