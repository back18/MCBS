using MCBS.Common.Services;
using MCBS.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QuanLib.Core;
using QuanLib.Downloader;
using QuanLib.Downloader.Services;
using QuanLib.IO.Extensions;
using QuanLib.Minecraft.Downloading;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.ConsoleTerminal.Services
{
    public class MinecraftResourceDownloader
    {
        private const string DEFAULT_LANGUAGE = "en_us";

        public MinecraftResourceDownloader(
            ILoggerProvider loggerProvider,
            IMinecraftConfigProvider configProvider,
            IMinecraftPathProvider pathProvider,
            IMinecraftDownloadProvider downloadProvider,
            IDownloadService downloadService,
            IAsyncHashComputeService hashComputeService,
            IDownloadProgressFormatter progressFormatter)
        {
            ArgumentNullException.ThrowIfNull(loggerProvider, nameof(loggerProvider));
            ArgumentNullException.ThrowIfNull(configProvider, nameof(configProvider));
            ArgumentNullException.ThrowIfNull(pathProvider, nameof(pathProvider));
            ArgumentNullException.ThrowIfNull(downloadProvider, nameof(downloadProvider));
            ArgumentNullException.ThrowIfNull(downloadService, nameof(downloadService));
            ArgumentNullException.ThrowIfNull(hashComputeService, nameof(hashComputeService));
            ArgumentNullException.ThrowIfNull(progressFormatter, nameof(progressFormatter));

            _logger = loggerProvider.GetLogger();
            _configProvider = configProvider;
            _pathProvider = pathProvider;
            _downloadProvider = downloadProvider;
            _downloadService = downloadService;
            _hashComputeService = hashComputeService;
            _progressFormatter = progressFormatter;
        }

        private readonly ILogger _logger;
        private readonly IMinecraftConfigProvider _configProvider;
        private readonly IMinecraftPathProvider _pathProvider;
        private readonly IMinecraftDownloadProvider _downloadProvider;
        private readonly IDownloadService _downloadService;
        private readonly IAsyncHashComputeService _hashComputeService;
        private readonly IDownloadProgressFormatter _progressFormatter;
        private int _requestCount;

        public async Task StartAsync()
        {
            VersionJson versionJson = await LoadVersionJsonAsync().ConfigureAwait(false);

            if (versionJson.GetIndexFile() is not NetworkAssetIndex indexFileAssetIndex)
                throw new InvalidDataException("找不到索引文件资源");
            if (versionJson.GetClientCore() is not NetworkAssetIndex clientCoreAssetIndex)
                throw new InvalidDataException("找不到客户端核心文件资源");

            await DownloadFileAsync(indexFileAssetIndex, _pathProvider.IndexFile).ConfigureAwait(false);
            await DownloadFileAsync(clientCoreAssetIndex, _pathProvider.ClientCore).ConfigureAwait(false);

            string language = _configProvider.Config.Language;
            if (language != DEFAULT_LANGUAGE)
            {
                AssetManifest assetManifest = await ReadAssetManifestAsync().ConfigureAwait(false);
                await DownloadLanguageFileAsync(assetManifest, language).ConfigureAwait(false);
            }

            if (_requestCount > 0)
                _logger.Info("Minecraft资源文件下载完成");
            else
                _logger.Info("Minecraft资源文件已通过完整性检查");

            _requestCount = 0;
        }

        private async Task<VersionJson> LoadVersionJsonAsync()
        {
            string versionJsonText;
            if (_pathProvider.VersionJson.ReadAllTextAsyncIfExists(out var versionJsonRead))
            {
                versionJsonText = await versionJsonRead.ConfigureAwait(false);
            }
            else
            {
                VersionManifest versionManifest = await DownloadVersionManifestAsync().ConfigureAwait(false);
                string version = _configProvider.Config.MinecraftVersion;

                if (!versionManifest.TryGetValue(version, out var versionIndex))
                    throw new InvalidDataException("找不到游戏版本资源: " + version);

                using Stream stream = await DownloadAsync(_downloadProvider.RedirectUrl(versionIndex.Url), _pathProvider.VersionJson.FullName, true).ConfigureAwait(false);
                using StreamReader reader = new(stream, Encoding.UTF8);
                versionJsonText = await reader.ReadToEndAsync().ConfigureAwait(false);
            }

            return new VersionJson(JObject.Parse(versionJsonText));
        }

        private async Task<VersionManifest> DownloadVersionManifestAsync()
        {
            using Stream stream = await DownloadAsync(_downloadProvider.VersionManifestUrl).ConfigureAwait(false);
            using StreamReader reader = new(stream, Encoding.UTF8);
            string text = await reader.ReadToEndAsync().ConfigureAwait(false);
            var model = JsonConvert.DeserializeObject<VersionManifest.Model>(text) ?? throw new InvalidDataException();
            return new VersionManifest(model);
        }

        private async Task<AssetManifest> ReadAssetManifestAsync()
        {
            using FileStream fileStream = _pathProvider.IndexFile.OpenRead();
            using StreamReader reader = new(fileStream, Encoding.UTF8);
            string text = await reader.ReadToEndAsync().ConfigureAwait(false);
            var model = JsonConvert.DeserializeObject<AssetManifest.Model>(text) ?? throw new InvalidDataException();
            return new AssetManifest(model);
        }

        private Task DownloadLanguageFileAsync(AssetManifest assetList, string language)
        {
            ArgumentNullException.ThrowIfNull(assetList, nameof(assetList));
            ArgumentException.ThrowIfNullOrEmpty(language, nameof(language));

            string langFileName = language + ".json";
            string langAssetPath = "minecraft/lang/" + langFileName;

            if (!assetList.TryGetValue(langAssetPath, out var langAssetIndex))
                throw new InvalidDataException("找不到语言文件资源: " + langAssetPath);

            return DownloadAssetAsync(langAssetIndex, _pathProvider.Languages.CombineFile(langFileName));
        }

        private async Task DownloadAssetAsync(AssetIndex assetIndex, FileInfo fileInfo)
        {
            ArgumentNullException.ThrowIfNull(assetIndex, nameof(assetIndex));
            ArgumentNullException.ThrowIfNull(fileInfo, nameof(fileInfo));

            bool valid = await ValidateFileAsync(assetIndex, fileInfo).ConfigureAwait(false);
            if (valid)
                return;

            await DownloadAsync(_downloadProvider.GetAssetUrl(assetIndex.Hash), fileInfo.FullName, false).ConfigureAwait(false);
        }

        private async Task DownloadFileAsync(NetworkAssetIndex networkAssetIndex, FileInfo fileInfo)
        {
            ArgumentNullException.ThrowIfNull(networkAssetIndex, nameof(networkAssetIndex));
            ArgumentNullException.ThrowIfNull(fileInfo, nameof(fileInfo));

            bool valid = await ValidateFileAsync(networkAssetIndex, fileInfo).ConfigureAwait(false);
            if (valid)
                return;

            await DownloadAsync(_downloadProvider.RedirectUrl(networkAssetIndex.Url), fileInfo.FullName, false).ConfigureAwait(false);
        }

        private async Task<bool> ValidateFileAsync(AssetIndex assetIndex, FileInfo fileInfo)
        {
            if (!fileInfo.Exists)
                return false;

            if (fileInfo.Length != assetIndex.Size)
                return false;

            using FileStream fileStream = fileInfo.OpenRead();
            string hash = await _hashComputeService.GetHashStringAsync(fileStream, assetIndex.HashType).ConfigureAwait(false);
            return string.Equals(hash, assetIndex.Hash, StringComparison.OrdinalIgnoreCase);
        }

        private Task<Stream> DownloadAsync(string url, CancellationToken cancellationToken = default)
        {
            _requestCount++;
            Progress<DownloadProgress> progress = new(OnProgressChanged);
            return _downloadService.DownloadAsync(url, progress, cancellationToken);
        }

        private Task<Stream> DownloadAsync(string url, string path, bool openFileStream, CancellationToken cancellationToken = default)
        {
            _requestCount++;
            Progress<DownloadProgress> progress = new(OnProgressChanged);
            return _downloadService.DownloadAsync(url, path, openFileStream, progress, cancellationToken);
        }

        private void OnProgressChanged(DownloadProgress progress)
        {
            _logger.Info(_progressFormatter.FormatProgress(progress));
        }
    }
}
