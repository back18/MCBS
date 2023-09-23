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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SixLabors.ImageSharp.PixelFormats;

namespace MCBS
{
    public static class MinecraftResourcesManager
    {
        private static readonly LogImpl LOGGER = LogUtil.MainLogger;

        public static void LoadAll()
        {
            LOGGER.Info("开始构建Minecraft资源文件");
            VersionDircetory directory = new(MCOS.MainDirectory.MinecraftResources.Vanilla.Combine(MinecraftConfig.GameVersion));
            BuildResourcesAsync(directory).Wait();
            LOGGER.Info("Minecraft资源文件构建完成");

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

        private static async Task BuildResourcesAsync(VersionDircetory directory)
        {
            directory.CreateIfNotExists();
            directory.Languages.CreateIfNotExists();

            DownloadProvider downloadProvider = MinecraftConfig.DownloadApi switch
            {
                Config.DownloadApis.MOJANG => DownloadProvider.MOJANG_PROVIDER,
                Config.DownloadApis.BMCLAPI => DownloadProvider.BMCLAPI_PROVIDER,
                _ => throw new InvalidOperationException()
            };

            string versionJsonText;
            if (File.Exists(directory.Version))
            {
                versionJsonText = File.ReadAllText(directory.Version);
            }
            else
            {
                LOGGER.Info("正在下载: " + downloadProvider.VersionListUrl);
                VersionList versionList = await VersionList.DownloadAsync(downloadProvider.VersionListUrl);

                if (!versionList.TryGetValue(MinecraftConfig.GameVersion, out var versionIndex))
                    throw new InvalidOperationException("未知的游戏版本：" + MinecraftConfig.GameVersion);

                string versionJsonUrl = downloadProvider.RedirectUrl(versionIndex.Url);
                LOGGER.Info("正在下载: " + versionJsonUrl);
                byte[] byees = await DownloadUtil.DownloadBytesAsync(versionJsonUrl);
                versionJsonText = Encoding.UTF8.GetString(byees);
                await File.WriteAllBytesAsync(directory.Version, byees);
            }

            VersionJson versionJson = new(JObject.Parse(versionJsonText));

            NetworkAssetIndex clientAssetIndex = versionJson.GetClientCore() ?? throw new InvalidOperationException("在版本Json文件找不到客户端核心文件的资源索引");
            await ReadOrDownloadAsync(directory.Client, clientAssetIndex, downloadProvider);

            NetworkAssetIndex indexFileAssetIndex = versionJson.GetIndexFile() ?? throw new InvalidOperationException("在版本Json文件找不到索引文件的资源索引");
            byte[] indexFileBytes = await ReadOrDownloadAsync(directory.Index, indexFileAssetIndex, downloadProvider);
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
                    string path = directory.Languages.Combine(asset.Key[lang.Length..]);
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
