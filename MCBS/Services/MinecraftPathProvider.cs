using QuanLib.IO.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.Services
{
    public class MinecraftPathProvider : IMinecraftPathProvider
    {
        public MinecraftPathProvider(IMinecraftConfigProvider configProvider)
        {
            ArgumentNullException.ThrowIfNull(configProvider, nameof(configProvider));

            _configProvider = configProvider;
        }

        private readonly IMinecraftConfigProvider _configProvider;

        public DirectoryInfo Minecraft => McbsPathManager.MCBS_Minecraft;

        public DirectoryInfo ResourcePacks => McbsPathManager.MCBS_Minecraft_ResourcePacks;

        public DirectoryInfo Languages => string.Format(McbsPathManager.Paths.MCBS_Minecraft_Vanilla_Version_Languages, _configProvider.Config.MinecraftVersion).CreateDirectoryInfo();

        public FileInfo VersionJson => string.Format(McbsPathManager.Paths.MCBS_Minecraft_Vanilla_Version_VersionJson, _configProvider.Config.MinecraftVersion).CreateFileInfo();

        public FileInfo ClientCore => string.Format(McbsPathManager.Paths.MCBS_Minecraft_Vanilla_Version_ClientCore, _configProvider.Config.MinecraftVersion).CreateFileInfo();

        public FileInfo IndexFile => string.Format(McbsPathManager.Paths.MCBS_Minecraft_Vanilla_Version_IndexFile, _configProvider.Config.MinecraftVersion).CreateFileInfo();
    }
}
