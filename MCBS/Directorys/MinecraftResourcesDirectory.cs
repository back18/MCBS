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
            ResourcePacks = new(Combine("ResourcePacks"));
            Vanilla = new(Combine("Vanilla"));
        }

        public ResourcePacksDirectory ResourcePacks { get; }

        public VanillaDirectory Vanilla { get; }
    }
}
