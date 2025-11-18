using static MCBS.Config.ConfigManager;
using log4net.Core;
using log4net.Repository.Hierarchy;
using Newtonsoft.Json.Linq;
using QuanLib.Minecraft.Downloading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using QuanLib.Minecraft.ResourcePack;
using QuanLib.Downloader;
using QuanLib.Core.Extensions;
using QuanLib.Core;
using QuanLib.IO.Zip;
using System.IO.Compression;
using QuanLib.Logging;
using QuanLib.IO;
using MCBS.Config.Constants;
using QuanLib.IO.Extensions;

namespace MCBS
{
    public static class MinecraftResourcesLoader
    {
        private static readonly LogImpl LOGGER = LogManager.Instance.GetLogger();

        public static ResourceEntryManager LoadAll()
        {
            BuildResourcesAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            LOGGER.Info("Minecraft资源文件构建完成");

            string[] resourcePacks = GetResourcePacks();
            string[] languageFiles = GetLanguageFiles();

            ZipPack[] zipPacks = new ZipPack[resourcePacks.Length];
            for (int i = 0; i < resourcePacks.Length; i++)
            {
                using FileStream fileStream = File.OpenRead(resourcePacks[i]);
                MemoryStream memoryStream = new();
                fileStream.CopyTo(memoryStream);
                zipPacks[i] = new(memoryStream, ZipArchiveMode.Update);
            }

            foreach (string languageFile in languageFiles)
            {
                using FileStream fileStream = File.OpenRead(languageFile);
                zipPacks[0].AddFile("/assets/minecraft/lang/" + Path.GetFileName(languageFile), fileStream);
            }

            ResourceEntryManager resources = ResourcePackReader.Load(zipPacks);

            LOGGER.Info("Minecraft资源文件加载完成");

            return resources;
        }

        private static string[] GetResourcePacks()
        {
            string[] result = new string[MinecraftConfig.ResourcePackList.Count + 1];
            result[0] = McbsPathManager.MCBS_Minecraft_Vanilla_Version_ClientCore.FullName;
            for (int i = 1; i < result.Length; i++)
                result[i] = McbsPathManager.MCBS_Minecraft_ResourcePacks.CombineFile(MinecraftConfig.ResourcePackList[i]).FullName;

            return result;
        }

        private static string[] GetLanguageFiles()
        {
            return McbsPathManager.MCBS_Minecraft_Vanilla_Version_Languages.GetFilePaths("*.json");
        }

        private static async Task BuildResourcesAsync()
        {
            DownloadProvider downloadProvider = MinecraftConfig.DownloadSource switch
            {
                DownloadSources.MOJANG => DownloadProvider.MOJANG_PROVIDER,
                DownloadSources.BMCLAPI => DownloadProvider.BMCLAPI_PROVIDER,
                _ => throw new InvalidOperationException()
            };

            string versionJsonText;
            if (McbsPathManager.MCBS_Minecraft_Vanilla_Version_VersionJson.Exists)
            {
                versionJsonText = await File.ReadAllTextAsync(McbsPathManager.MCBS_Minecraft_Vanilla_Version_VersionJson.FullName);
            }
            else
            {
                VersionList versionList = await DownloadVersionListAsync(downloadProvider.VersionListUrl);

                if (!versionList.TryGetValue(MinecraftConfig.MinecraftVersion, out var versionIndex))
                    throw new InvalidOperationException("未知的游戏版本: " + MinecraftConfig.MinecraftVersion);

                versionJsonText = await DownloadVersionJsonAsync(downloadProvider.RedirectUrl(versionIndex.Url), McbsPathManager.MCBS_Minecraft_Vanilla_Version_VersionJson.FullName);
            }

            VersionJson versionJson = new(JObject.Parse(versionJsonText));

            NetworkAssetIndex clientAssetIndex = versionJson.GetClientCore() ?? throw new InvalidOperationException("在版本Json文件找不到客户端核心文件的资源索引");
            (await ReadOrDownloadAsync(McbsPathManager.MCBS_Minecraft_Vanilla_Version_ClientCore.FullName, clientAssetIndex, downloadProvider)).Dispose();

            NetworkAssetIndex indexFileAssetIndex = versionJson.GetIndexFile() ?? throw new InvalidOperationException("在版本Json文件找不到索引文件的资源索引");
            AssetList assetList = await LoadAssetListAsync(McbsPathManager.MCBS_Minecraft_Vanilla_Version_IndexFile.FullName, indexFileAssetIndex, downloadProvider);

            if (MinecraftConfig.Language == "en_us")
                return;

            string langFileName = MinecraftConfig.Language + ".json";
            string langFilePath = McbsPathManager.MCBS_Minecraft_Vanilla_Version_Languages.CombineFile(langFileName).FullName;
            string langAssetPath = "minecraft/lang/" + langFileName;
            if (!assetList.TryGetValue(langAssetPath, out var langAssetIndex))
                throw new InvalidOperationException("未知的语言标识: " + MinecraftConfig.Language);

            if (!File.Exists(langFilePath) || HashUtil.GetHashString(langFilePath, HashType.SHA1) != langAssetIndex.Hash)
            {
                string langAssetUrl = downloadProvider.ToAssetUrl(langAssetIndex.Hash);
                await DownloadHelper.DownloadAsync(langAssetUrl, langFilePath);
            }
        }

        private static async Task<Stream> ReadOrDownloadAsync(string path, NetworkAssetIndex networkAssetIndex, DownloadProvider? downloadProvider = null)
        {
            ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));
            ArgumentNullException.ThrowIfNull(networkAssetIndex, nameof(networkAssetIndex));

            return await DownloadHelper.ReadOrDownloadAsync(downloadProvider?.RedirectUrl(networkAssetIndex.Url) ?? networkAssetIndex.Url, path, networkAssetIndex.Hash, HashType.SHA1);
        }

        private static async Task<VersionList> DownloadVersionListAsync(string url)
        {
            ArgumentException.ThrowIfNullOrEmpty(url, nameof(url));

            using Stream stream = await DownloadHelper.DownloadAsync(url);
            string text = stream.ReadAllText();
            var model = JsonConvert.DeserializeObject<VersionList.Model>(text) ?? throw new FormatException();
            return new(model);
        }

        private static async Task<string> DownloadVersionJsonAsync(string url, string path)
        {
            ArgumentException.ThrowIfNullOrEmpty(url, nameof(url));
            ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));

            using Stream stream = await DownloadHelper.DownloadAsync(url, path);
            string text = stream.ReadAllText();
            return text;
        }

        private static async Task<AssetList> LoadAssetListAsync(string path, NetworkAssetIndex networkAssetIndex, DownloadProvider? downloadProvider = null)
        {
            ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));
            ArgumentNullException.ThrowIfNull(networkAssetIndex, nameof(networkAssetIndex));

            using Stream stream = await ReadOrDownloadAsync(path, networkAssetIndex, downloadProvider);
            string text = stream.ReadAllText();
            var model = JsonConvert.DeserializeObject<AssetList.Model>(text) ?? throw new FormatException();
            return new(model);
        }

        private static string FormatProgress(DownloadManager downloadManager)
        {
            ArgumentNullException.ThrowIfNull(downloadManager, nameof(downloadManager));

            return $"多个文件下载中: {downloadManager.CompletedCount}/{downloadManager.Count}";
        }
    }
}
