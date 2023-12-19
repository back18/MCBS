using QuanLib.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Directorys
{
    public class MinecraftDirectory : DirectoryBase
    {
        public MinecraftDirectory(string directory) : base(directory)
        {
            ResourcePacksDir = AddDirectory<ResourcePacksDirectory>("ResourcePacks");
            VanillaDir = AddDirectory<VanillaDirectory>("Vanilla");
        }

        public ResourcePacksDirectory ResourcePacksDir { get; }

        public VanillaDirectory VanillaDir { get; }
    }
}
