using QuanLib.IO.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.Services.Implementations
{
    public class ScopedMinecraftPathProvider : IMinecraftPathProvider
    {
        public ScopedMinecraftPathProvider(string version)
        {
            ArgumentException.ThrowIfNullOrEmpty(version, nameof(version));

            _version = version;
        }

        private readonly string _version;

        public DirectoryInfo Minecraft => McbsPathManager.MCBS_Minecraft;

        public DirectoryInfo ResourcePacks => McbsPathManager.MCBS_Minecraft_ResourcePacks;

        public DirectoryInfo Languages => string.Format(McbsPathManager.Paths.MCBS_Minecraft_Vanilla_Version_Languages, _version).CreateDirectoryInfo();

        public FileInfo VersionJson => string.Format(McbsPathManager.Paths.MCBS_Minecraft_Vanilla_Version_VersionJson, _version).CreateFileInfo();

        public FileInfo ClientCore => string.Format(McbsPathManager.Paths.MCBS_Minecraft_Vanilla_Version_ClientCore, _version).CreateFileInfo();

        public FileInfo IndexFile => string.Format(McbsPathManager.Paths.MCBS_Minecraft_Vanilla_Version_IndexFile, _version).CreateFileInfo();
    }
}
