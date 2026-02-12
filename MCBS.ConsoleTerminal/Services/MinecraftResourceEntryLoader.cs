using MCBS.Services;
using QuanLib.Core;
using QuanLib.Core.Extensions;
using QuanLib.IO.Extensions;
using QuanLib.IO.Zip;
using QuanLib.Minecraft.ResourcePack;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Text;

namespace MCBS.ConsoleTerminal.Services
{
    public class MinecraftResourceEntryLoader
    {
        public MinecraftResourceEntryLoader(
            ILoggerProvider loggerProvider,
            IMinecraftPathProvider pathProvider,
            IMinecraftConfigProvider configProvider)
        {
            ArgumentNullException.ThrowIfNull(loggerProvider, nameof(loggerProvider));
            ArgumentNullException.ThrowIfNull(pathProvider, nameof(pathProvider));
            ArgumentNullException.ThrowIfNull(configProvider, nameof(configProvider));

            _logger = loggerProvider.GetLogger();
            _pathProvider = pathProvider;
            _configProvider = configProvider;
        }

        private readonly ILogger _logger;
        private readonly IMinecraftPathProvider _pathProvider;
        private readonly IMinecraftConfigProvider _configProvider;

        public async Task<ResourceEntryManager> LoadAsync()
        {
            ZipPack clientCorePack = await LoadClientCorePackAsync();
            FileInfo[] resourceFiles = GetResourceFiles();

            ResourceEntryManager resources;
            if (resourceFiles.Length > 0)
            {
                try
                {
                    ZipPack[] resourcePacks = await LoadResourcePacksAsync(resourceFiles);
                    resourcePacks = resourcePacks.LeftAddend(clientCorePack);
                    resources = ResourcePackReader.Load(resourcePacks);
                }
                catch (Exception ex)
                {
                    _logger.Error($"Minecraft资源包（{ObjectFormatter.Format(resourceFiles.Select(s => s.Name))}）加载失败", ex);
                    _logger.Info("尝试以安全模式仅加载原版资源包");
                    resources = ResourcePackReader.Load([clientCorePack]);
                }
            }
            else
            {
                resources = ResourcePackReader.Load([clientCorePack]);
            }

            _logger.Info($"Minecraft资源包加载完成，共加载 {resources.Count} 个资源包");
            return resources;
        }

        private async Task<ZipPack> LoadClientCorePackAsync()
        {
            using FileStream fileStream = _pathProvider.ClientCore.OpenRead();
            MemoryStream memoryStream = new();
            await fileStream.CopyToAsync(memoryStream);
            ZipPack zipPack = new(memoryStream, ZipArchiveMode.Update);

            foreach (string languagePath in _pathProvider.Languages.GetFilePaths())
            {
                using FileStream languageStream = File.OpenRead(languagePath);
                string languageName = "/assets/minecraft/lang/" + Path.GetFileName(languagePath);
                zipPack.AddFile(languageName, languageStream);
            }

            return zipPack;
        }

        private static async Task<ZipPack[]> LoadResourcePacksAsync(FileInfo[] files)
        {
            ArgumentNullException.ThrowIfNull(files, nameof(files));

            ZipPack[] resourcePacks = new ZipPack[files.Length];
            for (int i = 0; i < files.Length; i++)
            {
                using FileStream fileStream = files[i].OpenRead();
                MemoryStream memoryStream = new();
                await fileStream.CopyToAsync(memoryStream);
                resourcePacks[i] = new ZipPack(memoryStream, ZipArchiveMode.Read);
            }

            return resourcePacks;
        }

        private FileInfo[] GetResourceFiles()
        {
            var resources = _configProvider.Config.ResourcePackList;
            if (resources.Count == 0)
                return Array.Empty<FileInfo>();

            DirectoryInfo resourcePacks = _pathProvider.ResourcePacks;
            List<FileInfo> fileInfos = [];
            foreach (string resourceName in resources)
            {
                FileInfo fileInfo = resourcePacks.CombineFile(resourceName);
                if (fileInfo.Exists)
                    fileInfos.Add(fileInfo);
            }

            return fileInfos.ToArray();
        }
    }
}
