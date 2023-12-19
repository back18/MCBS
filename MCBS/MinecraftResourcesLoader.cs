using static MCBS.Config.ConfigManager;
using log4net.Core;
using log4net.Repository.Hierarchy;
using MCBS.Directorys;
using MCBS.Logging;
using Newtonsoft.Json.Linq;
using QuanLib.Minecraft.GameResource;
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

namespace MCBS
{
    public static class MinecraftResourcesLoader
    {
        private static readonly LogImpl LOGGER = LogUtil.GetLogger();

        public static ResourceEntryManager LoadAll()
        {
            while (true)
            {
                LOGGER.Info("开始构建Minecraft资源文件");
                try
                {
                    BuildResourcesAsync(MinecraftConfig.GameVersion).Wait();
                    LOGGER.Info("完成");
                    break;
                }
                catch (Exception ex)
                {
                    LOGGER.Error("构建失败，将在3秒后重试...", ex);
                    Thread.Sleep(3000);
                }
            }

            VersionDirectory directory = SR.McbsDirectory.MinecraftDir.VanillaDir.GetVersionDirectory(MinecraftConfig.GameVersion);
            string[] paths = new string[MinecraftConfig.ResourcePackList.Count + 1];
            paths[0] = directory.ClientFile;
            for (int i = 1; i < paths.Length; i++)
                paths[i] = SR.McbsDirectory.MinecraftDir.ResourcePacksDir.Combine(MinecraftConfig.ResourcePackList[i]);

            LOGGER.Info($"开始加载Minecraft资源包，共计{paths.Length}个资源包，资源包列表：");
            foreach (string path in paths)
                LOGGER.Info(Path.GetFileName(path));
            ResourceEntryManager resources = ResourcePackReader.Load(paths);
            LOGGER.Info("完成，资源包数量: " + resources.Count);

            return resources;
        }

        private static async Task<VersionDirectory> BuildResourcesAsync(string version)
        {
            ArgumentException.ThrowIfNullOrEmpty(version, nameof(version));

            VersionDirectory directory = SR.McbsDirectory.MinecraftDir.VanillaDir.GetVersionDirectory(version);
            directory.BuildDirectoryTree();

            DownloadProvider downloadProvider = MinecraftConfig.DownloadApi switch
            {
                Config.DownloadApis.MOJANG => DownloadProvider.MOJANG_PROVIDER,
                Config.DownloadApis.BMCLAPI => DownloadProvider.BMCLAPI_PROVIDER,
                _ => throw new InvalidOperationException()
            };

            string versionJsonText;
            if (File.Exists(directory.VersionFile))
            {
                versionJsonText = await File.ReadAllTextAsync(directory.VersionFile);
            }
            else
            {
                VersionList versionList = await DownloadVersionListAsync(downloadProvider.VersionListUrl);

                if (!versionList.TryGetValue(MinecraftConfig.GameVersion, out var versionIndex))
                    throw new InvalidOperationException("未知的游戏版本：" + MinecraftConfig.GameVersion);

                versionJsonText = await DownloadVersionJsonAsync(downloadProvider.RedirectUrl(versionIndex.Url), directory.VersionFile);
            }

            VersionJson versionJson = new(JObject.Parse(versionJsonText));

            NetworkAssetIndex clientAssetIndex = versionJson.GetClientCore() ?? throw new InvalidOperationException("在版本Json文件找不到客户端核心文件的资源索引");
            (await ReadOrDownloadAsync(directory.ClientFile, clientAssetIndex, downloadProvider)).Dispose();

            NetworkAssetIndex indexFileAssetIndex = versionJson.GetIndexFile() ?? throw new InvalidOperationException("在版本Json文件找不到索引文件的资源索引");
            AssetList assetList = await LoadAssetListAsync(directory.IndexFile, indexFileAssetIndex, downloadProvider);

            string langPath = "minecraft/lang/";
            DownloadManager downloadManager = new();
            foreach (var asset in assetList)
            {
                if (asset.Key.StartsWith(langPath))
                {
                    string path = directory.LanguagesDir.Combine(asset.Key[langPath.Length..]);
                    if (DownloadHelper.ReadIfValid(path, asset.Value.Hash, HashType.SHA1, out var fileStream))
                    {
                        fileStream.Dispose();
                        continue;
                    }

                    string url = downloadProvider.ToAssetUrl(asset.Value.Hash);
                    downloadManager.Add(url, path);
                    LOGGER.Info("已添加下载任务: " + url);
                }
            }

            if (downloadManager.Count > 0)
            {
                while (!downloadManager.IsAllCompleted)
                {
                    downloadManager.RetryAllIfFailed();
                    LOGGER.Info(FormatProgress(downloadManager));

                    try
                    {
                        await downloadManager.WaitAllTaskCompletedAsync().WaitAsync(TimeSpan.FromSeconds(1));
                    }
                    catch (TimeoutException)
                    {

                    }
                }
                LOGGER.Info(FormatProgress(downloadManager));

                downloadManager.ClearAll();
            }

            return directory;
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
            string text = stream.ToUtf8Text();
            var model = JsonConvert.DeserializeObject<VersionList.Model>(text) ?? throw new FormatException();
            return new(model);
        }

        private static async Task<string> DownloadVersionJsonAsync(string url, string path)
        {
            ArgumentException.ThrowIfNullOrEmpty(url, nameof(url));
            ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));

            using Stream stream = await DownloadHelper.DownloadAsync(url, path);
            string text = stream.ToUtf8Text();
            return text;
        }

        private static async Task<AssetList> LoadAssetListAsync(string path, NetworkAssetIndex networkAssetIndex, DownloadProvider? downloadProvider = null)
        {
            ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));
            ArgumentNullException.ThrowIfNull(networkAssetIndex, nameof(networkAssetIndex));

            using Stream stream = await ReadOrDownloadAsync(path, networkAssetIndex, downloadProvider);
            string text = stream.ToUtf8Text();
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
