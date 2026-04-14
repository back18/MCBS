using MCBS.Common;
using MCBS.Common.Services;
using MCBS.ConsoleTerminal.Services;
using MCBS.Services;
using Microsoft.Extensions.DependencyInjection;
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

namespace MCBS.ConsoleTerminal.Launcher
{
    public class MinecraftResourceDownloader
    {
        private const string DEFAULT_LANGUAGE = "en_us";

        public MinecraftResourceDownloader(
            ILoggerProvider loggerProvider,
            IMinecraftConfigProvider configProvider,
            IMinecraftPathProvider pathProvider,
            IMinecraftDownloadProvider downloadProvider,
            [FromKeyedServices("MANIFEST")] IManifestResourceProvider manifestResourceProvider,
            [FromKeyedServices("INDEX_FILE")] IVersionResourceProvider indexFileResourceProvider,
            [FromKeyedServices("CLIENT_CORE")] IVersionResourceProvider clientCoreResourceProvider,
            [FromKeyedServices("LANGUAGE")] IAssetResourceProvider languageResourceProvider,
            IDownloadService downloadService,
            IAsyncHashComputeService hashComputeService,
            IDownloadProgressFormatter progressFormatter)
        {
            ArgumentNullException.ThrowIfNull(loggerProvider, nameof(loggerProvider));
            ArgumentNullException.ThrowIfNull(configProvider, nameof(configProvider));
            ArgumentNullException.ThrowIfNull(pathProvider, nameof(pathProvider));
            ArgumentNullException.ThrowIfNull(downloadProvider, nameof(downloadProvider));
            ArgumentNullException.ThrowIfNull(manifestResourceProvider, nameof(manifestResourceProvider));
            ArgumentNullException.ThrowIfNull(indexFileResourceProvider, nameof(indexFileResourceProvider));
            ArgumentNullException.ThrowIfNull(clientCoreResourceProvider, nameof(clientCoreResourceProvider));
            ArgumentNullException.ThrowIfNull(languageResourceProvider, nameof(languageResourceProvider));
            ArgumentNullException.ThrowIfNull(downloadService, nameof(downloadService));
            ArgumentNullException.ThrowIfNull(hashComputeService, nameof(hashComputeService));
            ArgumentNullException.ThrowIfNull(progressFormatter, nameof(progressFormatter));

            _logger = loggerProvider.GetLogger();
            _configProvider = configProvider;
            _pathProvider = pathProvider;
            _downloadProvider = downloadProvider;
            _manifestResourceProvider = manifestResourceProvider;
            _indexFileResourceProvider = indexFileResourceProvider;
            _clientCoreResourceProvider = clientCoreResourceProvider;
            _languageResourceProvider = languageResourceProvider;
            _downloadService = downloadService;
            _hashComputeService = hashComputeService;
            _progressFormatter = progressFormatter;
        }

        private readonly ILogger _logger;
        private readonly IMinecraftConfigProvider _configProvider;
        private readonly IMinecraftPathProvider _pathProvider;
        private readonly IMinecraftDownloadProvider _downloadProvider;
        private readonly IManifestResourceProvider _manifestResourceProvider;
        private readonly IVersionResourceProvider _indexFileResourceProvider;
        private readonly IVersionResourceProvider _clientCoreResourceProvider;
        private readonly IAssetResourceProvider _languageResourceProvider;
        private readonly IDownloadService _downloadService;
        private readonly IAsyncHashComputeService _hashComputeService;
        private readonly IDownloadProgressFormatter _progressFormatter;
        private int _requestCount;

        public async Task StartAsync()
        {
            VersionJson versionJson = await LoadVersionJsonAsync().ConfigureAwait(false);
            DownloadAsset indexFileAsset = _indexFileResourceProvider.GetDownloadAsset(versionJson);
            DownloadAsset clientCoreAsset = _clientCoreResourceProvider.GetDownloadAsset(versionJson);

            await DownloadFileAsync(indexFileAsset, new FileInfo(indexFileAsset.FilePath)).ConfigureAwait(false);
            await DownloadFileAsync(clientCoreAsset, new FileInfo(clientCoreAsset.FilePath)).ConfigureAwait(false);

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
                DownloadAsset downloadAsset = _manifestResourceProvider.GetDownloadAsset(versionManifest, _configProvider.Config.MinecraftVersion);

                using Stream stream = await DownloadAsync(downloadAsset.Url, downloadAsset.FilePath, true).ConfigureAwait(false);
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

            DownloadAsset downloadAsset = _languageResourceProvider.GetDownloadAsset(assetList, language);
            FileInfo fileInfo = new(downloadAsset.FilePath);
            return DownloadFileAsync(downloadAsset, fileInfo);
        }

        private async Task DownloadFileAsync(DownloadAsset downloadAsset, FileInfo fileInfo)
        {
            ArgumentNullException.ThrowIfNull(downloadAsset, nameof(downloadAsset));
            ArgumentNullException.ThrowIfNull(fileInfo, nameof(fileInfo));

            bool valid = await ValidateFileAsync(downloadAsset, fileInfo).ConfigureAwait(false);
            if (valid)
                return;

            await DownloadAsync(_downloadProvider.RedirectUrl(downloadAsset.Url), fileInfo.FullName, false).ConfigureAwait(false);
        }

        private async Task<bool> ValidateFileAsync(DownloadAsset downloadAsset, FileInfo fileInfo)
        {
            if (!fileInfo.Exists)
                return false;

            if (downloadAsset.FileSize >= 0 && fileInfo.Length != downloadAsset.FileSize)
                return false;

            if (string.IsNullOrEmpty(downloadAsset.HashValue))
                return true;

            using FileStream fileStream = fileInfo.OpenRead();
            string hash = await _hashComputeService.GetHashStringAsync(fileStream, downloadAsset.HashType).ConfigureAwait(false);
            return string.Equals(hash, downloadAsset.HashValue, StringComparison.OrdinalIgnoreCase);
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
