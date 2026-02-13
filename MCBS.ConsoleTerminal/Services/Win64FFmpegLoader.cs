using FFMediaToolkit;
using MCBS.Common;
using MCBS.Common.Services;
using MCBS.Services;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using QuanLib.Core;
using QuanLib.Downloader;
using QuanLib.Downloader.Services;
using QuanLib.IO.Extensions;
using QuanLib.IO.Zip;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text;

namespace MCBS.ConsoleTerminal.Services
{
    public class Win64FFmpegLoader : IFFmpegLoader
    {
        public Win64FFmpegLoader(
            ILoggerProvider loggerProvider,
            IFFmpegPathProvider pathProvider,
            [FromKeyedServices(nameof(OSPlatform.Windows))] IFFmpegDownloadProvider downloadProvider,
            IDownloadService downloadService,
            IAsyncHashComputeService hashComputeService,
            IDownloadProgressFormatter progressFormatter)
        {
            ArgumentNullException.ThrowIfNull(loggerProvider, nameof(loggerProvider));
            ArgumentNullException.ThrowIfNull(pathProvider, nameof(pathProvider));
            ArgumentNullException.ThrowIfNull(downloadProvider, nameof(downloadProvider));
            ArgumentNullException.ThrowIfNull(downloadService, nameof(downloadService));
            ArgumentNullException.ThrowIfNull(hashComputeService, nameof(hashComputeService));
            ArgumentNullException.ThrowIfNull(progressFormatter, nameof(progressFormatter));

            _logger = loggerProvider.GetLogger();
            _pathProvider = pathProvider;
            _downloadProvider = downloadProvider;
            _downloadService = downloadService;
            _hashComputeService = hashComputeService;
            _progressFormatter = progressFormatter;
        }

        private readonly ILogger _logger;
        private readonly IFFmpegPathProvider _pathProvider;
        private readonly IFFmpegDownloadProvider _downloadProvider;
        private readonly IDownloadService _downloadService;
        private readonly IAsyncHashComputeService _hashComputeService;
        private readonly IDownloadProgressFormatter _progressFormatter;

        public async Task LoadAsync()
        {
            await DownloadFileIfNotExists().ConfigureAwait(false);
            await BuildFileAsync().ConfigureAwait(false);

            try
            {
                FFmpegLoader.FFmpegPath = _pathProvider.Bin.FullName;
                FFmpegLoader.LoadFFmpeg();
                _logger.Info("FFmpeg资源文件加载完成");
            }
            catch (Exception ex)
            {
                _logger.Error("FFmpeg资源文件加载失败，视频模块不可用", ex);
            }
        }

        private async Task BuildFileAsync()
        {
            if (_pathProvider.Win64FileManifest.ReadAllTextAsyncIfExists(out var textReadTask))
            {
                string json = await textReadTask.ConfigureAwait(false);
                var model = JsonConvert.DeserializeObject<Dictionary<string, FileMetadata.Model>>(json) ?? [];
                Dictionary<string, FileMetadata> manifest = model.ToDictionary(itme => itme.Key, itme => FileMetadata.FromDataModel(itme.Value));
                ZipPack? zipPack = null;

                try
                {
                    foreach (FileMetadata metadata in manifest.Values)
                    {
                        bool valid = await ValidateFileAsync(metadata, GetLocalFileInfo(metadata)).ConfigureAwait(false);
                        if (!valid)
                        {
                            zipPack ??= OpenZipFile();
                            await DecompressZipFileAsync(zipPack, GetZipFilePath(metadata)).ConfigureAwait(false);
                        }
                    }
                }
                finally
                {
                    zipPack?.Dispose();
                }

                _logger.Info("FFmpeg资源文件已通过完整性检查");
            }
            else
            {
                using ZipPack zipPack = OpenZipFile();
                var model = await DecompressZipFileAsync(zipPack).ConfigureAwait(false);
                string json = JsonConvert.SerializeObject(model, Formatting.Indented);
                using StreamWriter streamWriter = _pathProvider.Win64FileManifest.CreateText();
                await streamWriter.WriteAsync(json).ConfigureAwait(false);
                _logger.Info("FFmpeg资源文件解包完成");
            }
        }

        private async Task DownloadFileIfNotExists()
        {
            FileInfo fileInfo = _pathProvider.Win64ZipFile;
            if (!fileInfo.Exists || fileInfo.Length == 0)
                await DownloadAsync(_downloadProvider.DownloadUrl, fileInfo.FullName, false).ConfigureAwait(false);
        }

        private ZipPack OpenZipFile()
        {
            FileStream fileStream = _pathProvider.Win64ZipFile.OpenRead();
            return new ZipPack(fileStream, ZipArchiveMode.Read, Encoding.UTF8, false);
        }

        private async Task<Dictionary<string, FileMetadata.Model>> DecompressZipFileAsync(ZipPack zipPack)
        {
            Dictionary<string, FileMetadata.Model> manifest = [];
            string binPath = GetZipBinPath();
            string[] zipPaths = zipPack.GetFilePaths(binPath);

            foreach (string zipPath in zipPaths)
            {
                var metadata = await DecompressZipFileAsync(zipPack, zipPath).ConfigureAwait(false);
                manifest.Add(metadata.Name, metadata);
            }

            return manifest;
        }

        private async Task<FileMetadata.Model> DecompressZipFileAsync(ZipPack zipPack, string zipPath)
        {
            string fileName = Path.GetFileName(zipPath);

            _pathProvider.Bin.CreateIfNotExists();
            FileInfo fileInfo = _pathProvider.Bin.CombineFile(fileName);
            using FileStream fileStream = fileInfo.Create();

            ZipItem zipItem = zipPack.GetFile(zipPath);
            using Stream zipStream = zipItem.OpenStream();
            await zipStream.CopyToAsync(fileStream).ConfigureAwait(false);

            fileStream.Seek(0, SeekOrigin.Begin);
            string hash = await _hashComputeService.GetHashStringAsync(fileStream, HashType.SHA1).ConfigureAwait(false);

            _logger.Info("已还原FFmpeg文件: " + fileName);

            return new FileMetadata.Model()
            {
                Name = fileName,
                Length = (int)fileStream.Length,
                Hash = hash,
                HashType = HashType.SHA1.ToString()
            };
        }

        private async Task<bool> ValidateFileAsync(FileMetadata metadata, FileInfo fileInfo)
        {
            if (!fileInfo.Exists)
                return false;

            if (fileInfo.Length != metadata.Length)
                return false;

            using FileStream fileStream = fileInfo.OpenRead();
            string hash = await _hashComputeService.GetHashStringAsync(fileStream, metadata.HashType);
            return string.Equals(hash, metadata.Hash, StringComparison.OrdinalIgnoreCase);
        }

        private Task<Stream> DownloadAsync(string url, string path, bool openFileStream, CancellationToken cancellationToken = default)
        {
            Progress<DownloadProgress> progress = new(OnProgressChanged);
            return _downloadService.DownloadAsync(url, path, openFileStream, progress, cancellationToken);
        }

        private void OnProgressChanged(DownloadProgress progress)
        {
            _logger.Info(_progressFormatter.FormatProgress(progress));
        }

        private string GetZipBinPath()
        {
            return _downloadProvider.PackName + "/bin/";
        }

        private string GetZipFilePath(FileMetadata metadata)
        {
            return GetZipBinPath() + metadata.Name;
        }

        private FileInfo GetLocalFileInfo(FileMetadata metadata)
        {
            return _pathProvider.Bin.CombineFile(metadata.Name);
        }
    }
}
