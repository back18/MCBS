using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.Services
{
    public interface IMinecraftPathProvider
    {
        public DirectoryInfo Minecraft { get; }

        public DirectoryInfo ResourcePacks { get; }

        public DirectoryInfo Languages { get; }

        public FileInfo VersionJson { get; }

        public FileInfo ClientCore { get; }

        public FileInfo IndexFile { get; }
    }
}
