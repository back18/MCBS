using MCBS.Common;
using MCBS.Common.Services;
using MCBS.Drawing;
using MCBS.Services;
using Newtonsoft.Json;
using QuanLib.Core;
using QuanLib.Game;
using QuanLib.IO.Extensions;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace MCBS.ConsoleTerminal.Services
{
    public class ColorMappingCacheLoader : IColorMappingCacheLoader
    {
        public ColorMappingCacheLoader(
            ILoggerProvider loggerProvider,
            ICachePathProvider pathProvider,
            IColorMappingBuildService buildService,
            IColorMappingSerializer serializer,
            IColorMappingCacheFactory cacheFactory,
            IAsyncHashComputeService hashComputeService)
        {
            ArgumentNullException.ThrowIfNull(loggerProvider, nameof(loggerProvider));
            ArgumentNullException.ThrowIfNull(pathProvider, nameof(pathProvider));
            ArgumentNullException.ThrowIfNull(buildService, nameof(buildService));
            ArgumentNullException.ThrowIfNull(serializer, nameof(serializer));
            ArgumentNullException.ThrowIfNull(cacheFactory, nameof(cacheFactory));
            ArgumentNullException.ThrowIfNull(hashComputeService, nameof(hashComputeService));

            _logger = loggerProvider.GetLogger();
            _pathProvider = pathProvider;
            _buildService = buildService;
            _serializer = serializer;
            _cacheFactory = cacheFactory;
            _hashComputeService = hashComputeService;
        }

        private readonly ILogger _logger;
        private readonly ICachePathProvider _pathProvider;
        private readonly IColorMappingBuildService _buildService;
        private readonly IColorMappingSerializer _serializer;
        private readonly IColorMappingCacheFactory _cacheFactory;
        private readonly IAsyncHashComputeService _hashComputeService;

        public async Task<IColorMappingCache> LoadAsync(Facing facing, IColorFinder colorFinder)
        {
            Rgba32[] mapping;
            Rgba32[] colorSet = colorFinder.GetColorSet();
            bool valid = await ValidateCacheAsync(facing, colorSet).ConfigureAwait(false);

            if (valid)
            {
                mapping = await ReadAsync(facing).ConfigureAwait(false);
                _logger.Info($"成功读取方块[{facing.ToEnglishString()}]面颜色映射表");
            }
            else
            {
                _logger.Info($"开始构建方块[{facing.ToEnglishString()}]面颜色映射表，共计 {colorSet.Length} 种颜色");

                Stopwatch stopwatch = Stopwatch.StartNew();
                mapping = await BuildAsync(colorFinder).ConfigureAwait(false);
                stopwatch.Stop();

                _logger.Info($"完成！耗时 {(int)stopwatch.Elapsed.TotalSeconds} 秒");

                try
                {
                    await SaveAsync(facing, colorSet, mapping).ConfigureAwait(false);
                    _logger.Info("颜色映射表缓存已保存至本地");

                }
                catch (Exception ex)
                {
                    _logger.Error("颜色映射表缓存保存失败", ex);
                }
            }

            return _cacheFactory.CreateCache(mapping);
        }

        private async Task<bool> ValidateCacheAsync(Facing facing, Rgba32[] colorSet)
        {
            FileInfo cacheInfo = _pathProvider.GetColorMappingInfo(facing);
            FileInfo cacheFile = _pathProvider.GetColorMappingCache(facing);

            if (!cacheInfo.Exists)
                return false;

            if (!cacheInfo.ReadAllTextAsyncIfExists(Encoding.UTF8, out var textReadTask))
                return false;

            string text = await textReadTask.ConfigureAwait(false);
            var model = JsonConvert.DeserializeObject<ColorMappingInfo.Model>(text);

            if (model is null)
                return false;

            ColorMappingInfo colorMappingInfo = new(model);
            bool valid = await ValidateCacheFileAsync(colorMappingInfo, cacheFile).ConfigureAwait(false);
            if (!valid)
                return false;

            return ValidateCacheColorSet(colorMappingInfo, colorSet);
        }

        private async Task<bool> ValidateCacheFileAsync(ColorMappingInfo colorMappingInfo, FileInfo fileInfo)
        {
            if (!fileInfo.Exists)
                return false;

            if (fileInfo.Length != colorMappingInfo.Length)
                return false;

            using FileStream fileStream = fileInfo.OpenRead();
            string hash = await _hashComputeService.GetHashStringAsync(fileStream, colorMappingInfo.HashType).ConfigureAwait(false);
            return string.Equals(hash, colorMappingInfo.Hash, StringComparison.OrdinalIgnoreCase);
        }

        private static bool ValidateCacheColorSet(ColorMappingInfo colorMappingInfo, Rgba32[] colorSet)
        {
            return colorMappingInfo.ColorSet.Count == colorSet.Length &&
                   colorMappingInfo.ColorSet.SetEquals(colorSet);
        }

        private Task<Rgba32[]> BuildAsync(IColorFinder colorFinder)
        {
            Progress<BuildProgress> progress = new(OnProgressChanged);
            return _buildService.BuildAsync(colorFinder, progress);
        }

        private async Task<Rgba32[]> ReadAsync(Facing facing)
        {
            using FileStream fileStream = _pathProvider.GetColorMappingCache(facing).OpenRead();
            return await _serializer.DeserializeAsync(fileStream);
        }

        private async Task SaveAsync(Facing facing, Rgba32[] colorSet, Rgba32[] mapping)
        {
            string cacheInfo = _pathProvider.GetColorMappingInfo(facing).FullName;
            string cacheFile = _pathProvider.GetColorMappingCache(facing).FullName;

            using FileStream fileStream = new(cacheFile, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
            await _serializer.SerializeAsync(mapping, fileStream).ConfigureAwait(false);

            fileStream.Position = 0;
            string hash = await _hashComputeService.GetHashStringAsync(fileStream, HashType.SHA1).ConfigureAwait(false);

            var colorMappingInfo = new ColorMappingInfo(Path.GetFileName(cacheFile), (int)fileStream.Length, hash, HashType.SHA1, colorSet).ToDataModel();
            string text = JsonConvert.SerializeObject(colorMappingInfo, Formatting.Indented);

            await File.WriteAllTextAsync(cacheInfo, text, Encoding.UTF8).ConfigureAwait(false);
        }

        private void OnProgressChanged(BuildProgress progress)
        {
            _logger.Info(FormatProgress(progress));
        }

        private static string FormatProgress(BuildProgress progress)
        {
            ProgressBar progressBar = new(progress.TotalCount)
            {
                Current = progress.CompletedCount,
                Length = 20
            };

            return $"{progressBar} {Math.Round(progress.ProgressPercentage, 2)}% - {progress.CompletedCount}/{progress.TotalCount}";
        }
    }
}
