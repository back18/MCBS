using log4net.Core;
using MCBS.Rendering;
using Newtonsoft.Json;
using QuanLib.Core;
using QuanLib.IO;
using QuanLib.Downloader;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuanLib.Logging;
using QuanLib.Game;
using QuanLib.IO.Extensions;

namespace MCBS
{
    public static class ColorMappingCacheBuilder
    {
        private static LogImpl LOGGER => LogManager.Instance.GetLogger();

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

            BuildProgress totalProgress = new(256 * 256 * 256);
            List<BuildProgress> threadProgresses = [];
            List<Thread> threads = [];
            Rgba32[] mapping = new Rgba32[totalProgress.TotalCount];
            int processorCount = Math.Max(Environment.ProcessorCount - 1, 1);
            int countPerThread = (int)Math.Ceiling((double)totalProgress.TotalCount / processorCount);

            for (int i = 0; i < processorCount; i++)
            {
                int count = i;
                BuildProgress threadProgress = new(countPerThread);
                Thread thread = new(() => BuildRange(colorMatcher, mapping, countPerThread * count, countPerThread, threadProgress))
                {
                    Priority = ThreadPriority.Highest
                };

                threadProgresses.Add(threadProgress);
                threads.Add(thread);
                thread.Start();
            }

            while (true)
            {
                bool isCompleted = !threads.Any(a => a.IsAlive);
                if (isCompleted)
                    break;

                ReportProgress();
                Thread.Sleep(sleepMilliseconds);
            }

            foreach (var thread in threads)
                thread.Join();

            ReportProgress();

            return new(mapping);

            void ReportProgress()
            {
                totalProgress.CompletedCount = 0;
                foreach (BuildProgress threadProgress in threadProgresses)
                    totalProgress.CompletedCount += threadProgress.CompletedCount;
                onProgress?.Invoke(totalProgress);
            }
        }

        private static void BuildRange(ColorMatcher<Rgba32> colorMatcher, Rgba32[] mapping, int startIndex, int count, BuildProgress buildProgress)
        {
            int endIndex = Math.Min(startIndex + count, mapping.Length);
            for (int i = startIndex; i < endIndex; i++)
            {
                mapping[i] = colorMatcher.Find(ColorMappingCache.ToColor(i));
                buildProgress.CompletedCount++;
            }
        }

        private static string FormatBuildProgress(BuildProgress buildProgress)
        {
            ProgressBar progressBar = new(buildProgress.TotalCount)
            {
                Current = buildProgress.CompletedCount,
                Length = 20
            };

            return $"{progressBar} {Math.Round(buildProgress.Percentage, 2)}% - {buildProgress.CompletedCount}/{buildProgress.TotalCount}";
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

        private static string GetJsonFilePath(Facing facing) => McbsPathManager.MCBS_Caches_ColorMapping.CombineFile(facing.ToString() + ".json").FullName;

        private static string GetBinFilePath(Facing facing) => McbsPathManager.MCBS_Caches_ColorMapping.CombineFile(facing.ToString() + ".bin").FullName;

        private class BuildInfo
        {
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。

            public string CacheHash { get; set; }

            public string[] Colors { get; set; }

#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        }

        private class BuildProgress
        {
            public BuildProgress(int totalCount)
            {
                ThrowHelper.ArgumentOutOfMin(0, totalCount, nameof(totalCount));

                TotalCount = totalCount;
                CompletedCount = 0;
            }

            public readonly int TotalCount;

            public int CompletedCount;

            public double Percentage => TotalCount == 0 ? 0 : (double)CompletedCount / TotalCount * 100;
        }
    }
}
