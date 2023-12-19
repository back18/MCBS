using log4net.Core;
using MCBS.Logging;
using MCBS.Rendering;
using Newtonsoft.Json;
using QuanLib.Core;
using QuanLib.IO;
using QuanLib.Downloader;
using QuanLib.Minecraft;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS
{
    public static class ColorMappingCacheBuilder
    {
        private static LogImpl LOGGER => LogUtil.GetLogger();

        public static bool ReadIfValid(Facing facing, [MaybeNullWhen(false)] out ColorMappingCache result)
        {
            if (Validate(facing))
            {
                byte[] bytes = File.ReadAllBytes(GetBinFilePath(facing));
                result = new(bytes);
                return true;
            }

            result = null;
            return false;
        }

        public static ColorMappingCache ReadOrBuild(Facing facing)
        {
            string jsonPath = GetJsonFilePath(facing);
            string binPath = GetBinFilePath(facing);

            if (Validate(facing))
            {
                byte[] bytes = File.ReadAllBytes(binPath);
                return new(bytes);
            }
            else
            {
                LOGGER.Info($"开始构建方块[{facing.ToEnglishString()}]面颜色映射表");
                ColorMappingCache cache = BuildMappingCache(SR.Rgba32BlockMappings[facing].CreateColorMatcher<Rgba32>(), 1000, (buildProgress) =>
                {
                    LOGGER.Info(FormatBuildProgress(buildProgress));
                });

                byte[] bytes = cache.ToBytes();
                List<string> colors = new();
                foreach (Rgba32 color in SR.Rgba32BlockMappings[facing].Keys)
                    colors.Add(color.ToHex());

                BuildInfo buildInfo = new()
                {
                    CacheHash = HashUtil.GetHashString(bytes, HashType.SHA1),
                    Colors = colors.ToArray()
                };

                File.WriteAllBytes(binPath, bytes);
                File.WriteAllText(jsonPath, JsonConvert.SerializeObject(buildInfo));

                return cache;
            }
        }

        private static ColorMappingCache BuildMappingCache(ColorMatcher<Rgba32> colorMatcher, int sleepMilliseconds, Action<BuildProgress>? onProgress = null)
        {
            ArgumentNullException.ThrowIfNull(colorMatcher, nameof(colorMatcher));
            ThrowHelper.ArgumentOutOfMin(0, sleepMilliseconds, nameof(sleepMilliseconds));

            BuildProgress buildProgress = new();
            buildProgress.TotalCount = 256 * 256 * 256;
            Rgba32[] mapping = new Rgba32[buildProgress.TotalCount];

            bool completed = false;
            Task.Run(() =>
            {
                while (buildProgress.CompletedCount < buildProgress.TotalCount || !completed)
                {
                    onProgress?.Invoke(buildProgress);
                    Thread.Sleep(sleepMilliseconds);
                }
            });

            ParallelLoopResult parallelLoopResult = Parallel.For(0, buildProgress.TotalCount, (i) =>
            {
                mapping[i] = colorMatcher.Match(ColorMappingCache.ToColor(i));
                Interlocked.Increment(ref buildProgress.CompletedCount);
            });

            completed = true;
            return new(mapping);
        }

        private static string FormatBuildProgress(BuildProgress buildProgress)
        {
            return $"{DownloaderUtil.FormatProgressBar(buildProgress.TotalCount, buildProgress.CompletedCount, 20)} {Math.Round(buildProgress.Percentage, 2)}% - {buildProgress.CompletedCount}/{buildProgress.TotalCount}";
        }

        private static bool Validate(Facing facing)
        {
            string jsonPath = GetJsonFilePath(facing);
            if (!File.Exists(jsonPath))
                return false;

            BuildInfo? buildInfo = JsonConvert.DeserializeObject<BuildInfo>(File.ReadAllText(jsonPath));
            if (buildInfo is null)
                return false;

            HashSet<string> colors = new();
            foreach (Rgba32 color in SR.Rgba32BlockMappings[facing].Keys)
                colors.Add(color.ToHex());

            if (colors.Count != buildInfo.Colors.Length || !colors.SetEquals(buildInfo.Colors))
                return false;

            string binPath = GetBinFilePath(facing);
            if (!File.Exists(binPath))
                return false;

            string cacheHash = HashUtil.GetHashString(binPath, HashType.SHA1);
            if (buildInfo.CacheHash != cacheHash)
                return false;

            return true;
        }

        private static string GetJsonFilePath(Facing facing) => SR.McbsDirectory.CachesDir.ColorMappingDir.Combine(facing.ToString() + ".json");

        private static string GetBinFilePath(Facing facing) => SR.McbsDirectory.CachesDir.ColorMappingDir.Combine(facing.ToString() + ".bin");

        private class BuildInfo
        {
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。

            public string CacheHash { get; set; }

            public string[] Colors { get; set; }

#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        }

        private class BuildProgress
        {
            public BuildProgress()
            {
                TotalCount = 0;
                CompletedCount = 0;
            }

            public int TotalCount;

            public int CompletedCount;

            public double Percentage => TotalCount == 0 ? 0 : (double)CompletedCount / TotalCount * 100;
        }
    }
}
