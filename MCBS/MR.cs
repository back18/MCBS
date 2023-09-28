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
using System.IO;
using System.Security.Policy;
using QuanLib.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SixLabors.ImageSharp.PixelFormats;
using MCBS.Directorys;

namespace MCBS
{
    public static class MR
    {
        private static readonly LogImpl LOGGER = LogUtil.GetLogger();

        public static BlockTextureManager BlockTextureManager
        {
            get
            {
                if (_BlockTextureManager is null)
                    throw new InvalidOperationException();
                return _BlockTextureManager;
            }
        }
        private static BlockTextureManager? _BlockTextureManager;

        public static LanguageManager LanguageManager
        {
            get
            {
                if (_LanguageManager is null)
                    throw new InvalidOperationException();
                return _LanguageManager;
            }
        }
        private static LanguageManager? _LanguageManager;

        public static void LoadAll()
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

            VersionDirectory directory = SR.McbsDirectory.MinecraftResourcesDir.VanillaDir.GetVersionDirectory(MinecraftConfig.GameVersion);
            string[] paths = new string[MinecraftConfig.ResourcePackList.Count + 1];
            paths[0] = directory.ClientFile;
            for (int i = 1; i < paths.Length; i++)
                paths[i] = SR.McbsDirectory.MinecraftResourcesDir.ResourcePacksDir.Combine(MinecraftConfig.ResourcePackList[i]);

            LOGGER.Info($"开始加载Minecraft资源包，共计{paths.Length}个资源包，资源包列表：");
            foreach (string path in paths)
                LOGGER.Info(Path.GetFileName(path));
            ResourceEntryManager resources = ResourcePackReader.Load(paths);
            LOGGER.Info("完成，资源包数量: " + resources.Count);

            LOGGER.Info("开始加载Minecraft方块纹理");
            _BlockTextureManager = BlockTextureManager.LoadInstance(resources, MinecraftConfig.BlockTextureBlacklist);
            LOGGER.Info("完成，方块数量: " + _BlockTextureManager.Count);

            string? minecraftLanguageFilePath = directory.LanguagesDir.Combine(MinecraftConfig.Language + ".json");
            if (!File.Exists(minecraftLanguageFilePath))
                minecraftLanguageFilePath = null;

            LOGGER.Info("开始加载Minecraft语言文件，语言标识: " + MinecraftConfig.Language);
            _LanguageManager = LanguageManager.LoadInstance(resources, MinecraftConfig.Language, minecraftLanguageFilePath);
            LOGGER.Info("完成，语言条目数量: " + _LanguageManager.Count);

            resources.Dispose();
            LOGGER.Info("资源包内所有资源均已加载完成，资源包缓存已释放");
        }

        private static async Task<VersionDirectory> BuildResourcesAsync(string version)
        {
            if (string.IsNullOrEmpty(version))
                throw new ArgumentException($"“{nameof(version)}”不能为 null 或空。", nameof(version));

            VersionDirectory directory = SR.McbsDirectory.MinecraftResourcesDir.VanillaDir.GetVersionDirectory(version);
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
                versionJsonText = File.ReadAllText(directory.VersionFile);
            }
            else
            {
                VersionList versionList;
                try
                {
                    LOGGER.Info("正在下载: " + downloadProvider.VersionListUrl);
                    versionList = await VersionList.DownloadAsync(downloadProvider.VersionListUrl);
                }
                catch (Exception ex)
                {
                    LOGGER.Warn("下载或解析失败，尝试切换到MOJANG官方下载源", ex);
                    LOGGER.Info("正在下载: " + DownloadProvider.MOJANG_PROVIDER.VersionListUrl);
                    versionList = await VersionList.DownloadAsync(DownloadProvider.MOJANG_PROVIDER.VersionListUrl);
                }

                if (!versionList.TryGetValue(MinecraftConfig.GameVersion, out var versionIndex))
                    throw new InvalidOperationException("未知的游戏版本：" + MinecraftConfig.GameVersion);

                string versionJsonUrl = downloadProvider.RedirectUrl(versionIndex.Url);
                LOGGER.Info("正在下载: " + versionJsonUrl);
                byte[] byees = await DownloadUtil.DownloadBytesAsync(versionJsonUrl);
                versionJsonText = Encoding.UTF8.GetString(byees);
                await File.WriteAllBytesAsync(directory.VersionFile, byees);
            }

            VersionJson versionJson = new(JObject.Parse(versionJsonText));

            NetworkAssetIndex clientAssetIndex = versionJson.GetClientCore() ?? throw new InvalidOperationException("在版本Json文件找不到客户端核心文件的资源索引");
            await ReadOrDownloadAsync(directory.ClientFile, clientAssetIndex, downloadProvider);

            NetworkAssetIndex indexFileAssetIndex = versionJson.GetIndexFile() ?? throw new InvalidOperationException("在版本Json文件找不到索引文件的资源索引");
            byte[] indexFileBytes = await ReadOrDownloadAsync(directory.IndexFile, indexFileAssetIndex, downloadProvider);
            string indexFileText = Encoding.UTF8.GetString(indexFileBytes);
            AssetList assetList = new(JsonConvert.DeserializeObject<AssetList.Model>(indexFileText) ?? throw new FormatException());

            string lang = "minecraft/lang/";
            int langCount = 0;
            List<Task> langTasks = new();
            foreach (var asset in assetList)
            {
                if (asset.Key.StartsWith(lang))
                {
                    string url = downloadProvider.ToAssetUrl(asset.Value.Hash);
                    string path = directory.LanguagesDir.Combine(asset.Key[lang.Length..]);
                    NetworkAssetIndex networkAssetIndex = new(asset.Value.Hash, asset.Value.Size, url);
                    langTasks.Add(ReadOrDownloadAsync(path, networkAssetIndex, downloadProvider).ContinueWith((task) => Interlocked.Increment(ref langCount)));
                }
            }

            Task langTask = Task.WhenAll(langTasks.ToArray());
            string empty = new(' ', 40);
            int point = 0;
            Console.CursorVisible = false;
            while (true)
            {
                Console.CursorLeft = 0;
                Console.Write(empty);
                Console.CursorLeft = 0;
                Console.Write($"共计{langTasks.Count}个语言文件，已加载{langCount}个" + new string('.', point++));
                if (point > 3)
                    point = 0;
                if (langCount >= langTasks.Count && langTask.IsCompleted)
                    break;
                else
                    Thread.Sleep(500);
            }
            Console.CursorVisible = true;
            Console.WriteLine();

            return directory;
        }

        private static async Task<byte[]> ReadOrDownloadAsync(string path, NetworkAssetIndex networkAssetIndex, DownloadProvider? downloadProvider = null)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException($"“{nameof(path)}”不能为 null 或空。", nameof(path));
            if (networkAssetIndex is null)
                throw new ArgumentNullException(nameof(networkAssetIndex));

            if (File.Exists(path))
            {
                byte[] bytes1 = await File.ReadAllBytesAsync(path);
                string hash = HashUtil.GetHashString(bytes1, HashType.SHA1);
                if (hash == networkAssetIndex.Hash)
                    return bytes1;
            }

            LOGGER.Info("正在下载: " + downloadProvider?.RedirectUrl(networkAssetIndex.Url) ?? networkAssetIndex.Url);
            byte[] bytes2 = await networkAssetIndex.DownloadBytesAsync(downloadProvider);
            await File.WriteAllBytesAsync(path, bytes2);
            return bytes2;
        }
    }
}
