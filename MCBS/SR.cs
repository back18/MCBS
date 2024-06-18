using static MCBS.Config.ConfigManager;
using log4net.Core;
using MCBS.Cursor.Style;
using QuanLib.BDF;
using QuanLib.Minecraft.ResourcePack;
using QuanLib.Minecraft.ResourcePack.Block;
using QuanLib.Minecraft.ResourcePack.Language;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using QuanLib.Logging;
using QuanLib.Game;
using MCBS.Drawing;

namespace MCBS
{
    public static class SR
    {
        private static LogImpl LOGGER => LogManager.Instance.GetLogger();

        public static ReadOnlyDictionary<Facing, Rgba32BlockMapping> Rgba32BlockMappings => _Rgba32BlockMappings ?? new(new Dictionary<Facing, Rgba32BlockMapping>());
        private static ReadOnlyDictionary<Facing, Rgba32BlockMapping>? _Rgba32BlockMappings;

        public static HashBlockMapping HashBlockMapping => _HashBlockMapping ?? throw new InvalidOperationException();
        private static HashBlockMapping? _HashBlockMapping;

        public static ReadOnlyDictionary<Facing, IColorMappingCache> ColorMappingCaches => _ColorMappingCaches ?? new(new Dictionary<Facing, IColorMappingCache>());
        private static ReadOnlyDictionary<Facing, IColorMappingCache>? _ColorMappingCaches;

        public static LanguageManager LanguageManager => _LanguageManager ?? throw new InvalidOperationException();
        private static LanguageManager? _LanguageManager;

        public static BdfFont DefaultFont => _DefaultFont ?? throw new InvalidOperationException();
        private static BdfFont? _DefaultFont;

        public static CursorStyleManager CursorStyleManager => _CursorStyleManager ?? throw new InvalidOperationException();
        private static CursorStyleManager? _CursorStyleManager;

        public static void LoadAll(ResourceEntryManager resources)
        {
            ArgumentNullException.ThrowIfNull(resources, nameof(resources));

            LoadBlockTextureManager(resources);
            BuildColorMappingCache(resources);
            LoadMinecraftLanguage(resources);
            LoadFontFile(resources);
            LoadCursorFile(resources);
        }

        private static void LoadBlockTextureManager(ResourceEntryManager resources)
        {
            using BlockTextureManager blockTextureManager = BlockTextureReader.Load(resources);
            Dictionary<Facing, Rgba32BlockMapping> mappings = new()
            {
                { Facing.Xp, new(blockTextureManager, Facing.Xp, ScreenConfig.ScreenBlockBlacklist) },
                { Facing.Xm, new(blockTextureManager, Facing.Xm, ScreenConfig.ScreenBlockBlacklist) },
                { Facing.Yp, new(blockTextureManager, Facing.Yp, ScreenConfig.ScreenBlockBlacklist) },
                { Facing.Ym, new(blockTextureManager, Facing.Ym, ScreenConfig.ScreenBlockBlacklist) },
                { Facing.Zp, new(blockTextureManager, Facing.Zp, ScreenConfig.ScreenBlockBlacklist) },
                { Facing.Zm, new(blockTextureManager, Facing.Zm, ScreenConfig.ScreenBlockBlacklist) }
            };

            _Rgba32BlockMappings = mappings.AsReadOnly();
            _HashBlockMapping = new();

            LOGGER.Info($"Minecraft方块纹理数据加载完成，成功加载 {blockTextureManager.Count} 个方块纹理数据");
        }

        private static void BuildColorMappingCache(ResourceEntryManager resources)
        {
            Dictionary<Facing, IColorMappingCache> caches = new();
            foreach (Facing facing in Enum.GetValues(typeof(Facing)))
            {
                IColorMappingCache? cache;
                if (SystemConfig.BuildColorMappingCaches)
                {
                    cache = ColorMappingCacheBuilder.ReadOrBuild(facing, SystemConfig.EnableCompressionCache);
                }
                else
                {
                    if (!ColorMappingCacheBuilder.ReadIfValid(facing, SystemConfig.EnableCompressionCache, out cache))
                        continue;
                }

                caches.Add(facing, cache);
            }
            _ColorMappingCaches = new(caches);

            LOGGER.Info("Minecraft方块颜色映射表缓存构建完成");
        }

        private static void LoadMinecraftLanguage(ResourceEntryManager resources)
        {
            _LanguageManager = LanguageManager.LoadInstance(new(resources, MinecraftConfig.Language));

            LOGGER.Info($"Minecraft语言数据加载完成，语言:{MinecraftConfig.Language} 条目数量:{_LanguageManager.Count}");
        }

        private static void LoadFontFile(ResourceEntryManager resources)
        {
            using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("MCBS.SystemResource.DefaultFont.bdf") ?? throw new InvalidOperationException();
            _DefaultFont = BdfFont.Load(stream);

            LOGGER.Info($"默认字体数据完成，字符数量:{_DefaultFont.Count} 字体高度:{_DefaultFont.Height} 全角宽度:{_DefaultFont.FullWidth} 半角宽度:{_DefaultFont.HalfWidth}");
        }

        private static void LoadCursorFile(ResourceEntryManager resources)
        {
            _CursorStyleManager = CursorStyleManager.LoadInstance();

            LOGGER.Info($"光标数据加载完成，光标数量:{_CursorStyleManager.Count}");
        }
    }
}
