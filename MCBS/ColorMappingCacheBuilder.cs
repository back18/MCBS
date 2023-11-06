using MCBS.Rendering;
using Newtonsoft.Json;
using QuanLib.Core.IO;
using QuanLib.Minecraft;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS
{
    public static class ColorMappingCacheBuilder
    {
        public static ColorMappingCache Build(Facing facing)
        {
            string jsonPath = GetJsonPath(facing);
            string binPath = GetBinPath(facing);

            if (Validate(facing))
            {
                byte[] bytes = File.ReadAllBytes(binPath);
                return new(bytes);
            }
            else
            {
                ColorMappingCache cache = SR.Rgba32BlockMappings[facing].CreateColorMatcher<Rgba32>().BuildMappingCache();

                byte[] bytes = cache.ToBytes();
                List<int> colors = new();
                foreach (Rgba32 color in SR.Rgba32BlockMappings[facing].Keys)
                    colors.Add(ColorMappingCache.ToIndex(color));

                BuildInfo buildInfo = new()
                {
                    CacheHash = HashUtil.GetHashString(bytes, HashType.SHA1),
                    Colors = colors.ToArray()
                };

                File.WriteAllText(jsonPath, JsonConvert.SerializeObject(buildInfo));
                File.WriteAllBytes(binPath, bytes);

                return cache;
            }
        }

        private static bool Validate(Facing facing)
        {
            string jsonPath = GetJsonPath(facing);
            if (!File.Exists(jsonPath))
                return false;

            BuildInfo? buildInfo = JsonConvert.DeserializeObject<BuildInfo>(File.ReadAllText(jsonPath));
            if (buildInfo is null)
                return false;

            HashSet<int> colors = new();
            foreach (Rgba32 color in SR.Rgba32BlockMappings[facing].Keys)
                colors.Add(ColorMappingCache.ToIndex(color));

            if (!colors.SetEquals(buildInfo.Colors))
                return false;

            string binPath = GetBinPath(facing);
            if (!File.Exists(binPath))
                return false;

            string cacheHash = HashUtil.GetHashString(binPath, HashType.SHA1);
            if (buildInfo.CacheHash != cacheHash)
                return false;

            return true;
        }

        private static string GetJsonPath(Facing facing) => SR.McbsDirectory.CachesDir.ColorMappingDir.Combine(facing.ToString() + ".json");
        
        private static string GetBinPath(Facing facing) => SR.McbsDirectory.CachesDir.ColorMappingDir.Combine(facing.ToString() + ".bin");

        private class BuildInfo
        {
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。

            public string CacheHash { get; set; }

            public int[] Colors { get; set; }

#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        }
    }
}
