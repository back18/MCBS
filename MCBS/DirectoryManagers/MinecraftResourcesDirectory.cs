using QuanLib.Core.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.DirectoryManagers
{
    public class MinecraftResourcesDirectory : DirectoryManager
    {
        public MinecraftResourcesDirectory(string directory) : base(directory)
        {
            ResourcePacks = new(Combine("ResourcePacks"));
            Languages = new(Combine("Languages"));
        }

        public ResourcePacksDirectory ResourcePacks { get; }

        public LanguagesDirectory Languages { get; }
    }
}
