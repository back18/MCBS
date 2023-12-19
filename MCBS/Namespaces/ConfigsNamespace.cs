using QuanLib.Core.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Namespaces
{
    public class ConfigsNamespace : NamespaceBase
    {
        public ConfigsNamespace(string @namespace) : base(@namespace)
        {
            Log4NetConfigFile = Combine("log4net.xml");
            MinecraftConfigFile = Combine("Minecraft.toml");
            RegistryConfigFile = Combine("Registry.json");
            ScreenConfigFile = Combine("Screen.toml");
            SystemConfigFile = Combine("System.toml");
        }

        public string Log4NetConfigFile { get; }

        public string MinecraftConfigFile { get; }

        public string RegistryConfigFile { get; }

        public string ScreenConfigFile { get; }

        public string SystemConfigFile { get; }
    }
}
