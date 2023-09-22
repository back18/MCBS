using static MCBS.Config.ConfigManager;
using log4net.Core;
using QuanLib.Minecraft.ResourcePack;
using QuanLib.Minecraft.ResourcePack.Block;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuanLib.Minecraft.ResourcePack.Language;
using MCBS.Logging;
using QuanLib.Minecraft.GameResource;
using NAudio.SoundFont;
using QuanLib.Core.IO;
using MCBS.DirectoryManagers;
using System.IO;
using System.Security.Policy;
using QuanLib.Core;

namespace MCBS
{
    public static class MinecraftResourcesManager
    {
        private static readonly LogImpl LOGGER = LogUtil.MainLogger;

        public static void LoadAll()
        {
            VersionDircetory directory = new(MCOS.MainDirectory.MinecraftResources.Vanilla.Combine(MinecraftConfig.GameVersion));
            if (!directory.Exists())
                DownloadResources(directory).Wait();

            string[] paths = new string[MinecraftConfig.ResourcePackList.Count + 1];
            paths[0] = directory.Client;
            for (int i = 1; i < paths.Length; i++)
                paths[i] = MCOS.MainDirectory.MinecraftResources.ResourcePacks.Combine(MinecraftConfig.ResourcePackList[i]);

            LOGGER.Info("开始加载Minecraft资源包");
            LOGGER.Info($"共计{paths.Length}个资源包，资源包列表：");
            foreach (string path in paths)
                LOGGER.Info(Path.GetFileName(path));
            ResourceEntryManager resources = ResourcePackReader.Load(paths);
            LOGGER.Info("Minecraft资源包加载完成，模组数量: " + resources.Count);

            LOGGER.Info("开始加载Minecraft方块纹理");
            LOGGER.Info($"已禁用{MinecraftConfig.BlockTextureBlacklist.Count}个方块纹理，方块纹理黑名单列表：");
            foreach (var blockState in MinecraftConfig.BlockTextureBlacklist)
                LOGGER.Info(blockState);
            BlockTextureManager.LoadInstance(resources, MinecraftConfig.BlockTextureBlacklist);
            LOGGER.Info($"Minecraft方块纹理文件加载完成，方块数量: " + BlockTextureManager.Instance.Count);

            string? minecraftLanguageFilePath = directory.Languages.Combine(MinecraftConfig.Language + ".json");
            if (!File.Exists(minecraftLanguageFilePath))
                minecraftLanguageFilePath = null;

            LOGGER.Info("开始加载Minecraft语言文件，语言标识: " + MinecraftConfig.Language);
            LanguageManager.LoadInstance(resources, MinecraftConfig.Language, minecraftLanguageFilePath);
            LOGGER.Info("Minecraft语言文件加载完成，语言条目数量: " + LanguageManager.Instance.Count);

            resources.Dispose();
        }

        private static async Task DownloadResources(VersionDircetory directory)
        {
            directory.CreateIfNotExists();
            directory.Languages.CreateIfNotExists();

            DownloadProvider downloadProvider = MinecraftConfig.DownloadApi switch
            {
                Config.DownloadApis.MOJANG => DownloadProvider.MOJANG_PROVIDER,
                Config.DownloadApis.BMCLAPI => DownloadProvider.BMCLAPI_PROVIDER,
                _ => throw new InvalidOperationException()
            };

            LOGGER.Info("正在下载: " + downloadProvider.VersionListUrl);
            VersionList versionList = await VersionList.DownloadAsync(downloadProvider.VersionListUrl);

            if (!versionList.TryGetValue(MinecraftConfig.GameVersion, out var versionIndex))
                throw new InvalidOperationException("未知的游戏版本：" + MinecraftConfig.GameVersion);

            string versionJsonUrl = downloadProvider.RedirectUrl(versionIndex.Url);
            LOGGER.Info("正在下载: " + versionJsonUrl);
            VersionJson versionJson = await VersionJson.DownloadAsync(versionJsonUrl);

            NetworkAssetIndex clientAssetIndex = versionJson.GetClientCore() ?? throw new InvalidOperationException("在版本Json文件找不到客户端核心资源索引");
            await ConditionalDownload(directory.Client, clientAssetIndex, downloadProvider);

            NetworkAssetIndex indexFileAssetIndex = versionJson.GetIndexFile() ?? throw new InvalidOperationException("在版本Json文件找不到索引文件的资源索引");
            string assetListUrl = downloadProvider.RedirectUrl(indexFileAssetIndex.Url);
            LOGGER.Info("正在下载: " + assetListUrl);
            AssetList assetList = await AssetList.DownloadAsync(assetListUrl);

            string start = "minecraft/lang/";
            foreach (var asset in assetList)
            {
                if (asset.Key.StartsWith(start))
                {
                    string url = downloadProvider.ToAssetUrl(asset.Value.Hash);
                    string path = directory.Languages.Combine(asset.Key[start.Length..]);
                    NetworkAssetIndex networkAssetIndex = new(asset.Value.Hash, asset.Value.Size, url);
                    await ConditionalDownload(path, networkAssetIndex, downloadProvider);
                }
            }
        }

        private static async Task ConditionalDownload(string path, NetworkAssetIndex networkAssetIndex, DownloadProvider? downloadProvider = null)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException($"“{nameof(path)}”不能为 null 或空。", nameof(path));
            if (networkAssetIndex is null)
                throw new ArgumentNullException(nameof(networkAssetIndex));

            if (File.Exists(path) && HashUtil.GetHashString(path, HashType.SHA1) == networkAssetIndex.Hash)
                return;

            LOGGER.Info("正在下载: " + downloadProvider?.RedirectUrl(networkAssetIndex.Url) ?? networkAssetIndex.Url);
            byte[] bytes = await networkAssetIndex.DownloadBytesAsync(downloadProvider);
            await File.WriteAllBytesAsync(path, bytes);
        }
    }
}
