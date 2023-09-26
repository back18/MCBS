using QuanLib.Core.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Directorys
{
    public class MinecraftResourcesDirectory : DirectoryManager
    {
        public MinecraftResourcesDirectory(string directory) : base(directory)
        {
            ResourcePacksDir = AddDirectory<ResourcePacksDirectory>("ResourcePacks");
            VanillaDir = AddDirectory<VanillaDirectory>("Vanilla");
        }

        public ResourcePacksDirectory ResourcePacksDir { get; }

        public VanillaDirectory VanillaDir { get; }
    }
}
