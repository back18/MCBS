using static MCBS.Config.ConfigManager;
using log4net.Core;
using MCBS.Cursor.Style;
using MCBS.Directorys;
using MCBS.Namespaces;
using QuanLib.BDF;
using QuanLib.Minecraft.ResourcePack;
using QuanLib.Minecraft.ResourcePack.Block;
using QuanLib.Minecraft.ResourcePack.Language;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MCBS.Rendering;
using System.Collections.ObjectModel;
using QuanLib.Minecraft;
using QuanLib.Logging;

namespace MCBS
{
    public static class SR
    {
        private static LogImpl LOGGER => LogManager.Instance.GetLogger();

        static SR()
        {
            SystemResourceNamespace = new("MCBS.SystemResource");
            McbsDirectory = new(Path.GetFullPath("MCBS"));
            McbsDirectory.BuildDirectoryTree();
        }

        public static SystemResourceNamespace SystemResourceNamespace { get; }

        public static McbsDirectory McbsDirectory { get; }

        public static BlockTextureManager BlockTextureManager => _BlockTextureManager ?? throw new InvalidOperationException();
        private static BlockTextureManager? _BlockTextureManager;

        public static ReadOnlyDictionary<Facing, Rgba32BlockMapping> Rgba32BlockMappings => _Rgba32BlockMappings ?? new(new Dictionary<Facing, Rgba32BlockMapping>());
        private static ReadOnlyDictionary<Facing, Rgba32BlockMapping>? _Rgba32BlockMappings;

        public static HashBlockMapping HashBlockMapping => _HashBlockMapping ?? throw new InvalidOperationException();
        private static HashBlockMapping? _HashBlockMapping;

        public static ReadOnlyDictionary<Facing, ColorMappingCache> ColorMappingCaches => _ColorMappingCaches ?? new(new Dictionary<Facing, ColorMappingCache>());
        private static ReadOnlyDictionary<Facing, ColorMappingCache>? _ColorMappingCaches;

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
            _BlockTextureManager = BlockTextureManager.LoadInstance(new(resources, MinecraftConfig.BlockTextureBlacklist));
            Dictionary<Facing, Rgba32BlockMapping> mappings = new()
            {
                { Facing.Xp, new(_BlockTextureManager, Facing.Xp) },
                { Facing.Xm, new(_BlockTextureManager, Facing.Xm) },
                { Facing.Yp, new(_BlockTextureManager, Facing.Yp) },
                { Facing.Ym, new(_BlockTextureManager, Facing.Ym) },
                { Facing.Zp, new(_BlockTextureManager, Facing.Zp) },
                { Facing.Zm, new(_BlockTextureManager, Facing.Zm) }
            };
            _Rgba32BlockMappings = new(mappings);
            _HashBlockMapping = new();

            LOGGER.Info($"Minecraft方块纹理数据加载完成，成功加载 {_BlockTextureManager.Count} 个方块纹理数据");
        }

        private static void BuildColorMappingCache(ResourceEntryManager resources)
        {
            Dictionary<Facing, ColorMappingCache> caches = new();
            foreach (Facing facing in Enum.GetValues(typeof(Facing)))
            {
                ColorMappingCache? cache;
                if (SystemConfig.BuildColorMappingCaches)
                {
                    cache = ColorMappingCacheBuilder.ReadOrBuild(facing);
                }
                else
                {
                    if (!ColorMappingCacheBuilder.ReadIfValid(facing, out cache))
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
            using Stream defaultFontStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(SystemResourceNamespace.DefaultFontFile) ?? throw new InvalidOperationException();
            _DefaultFont = BdfFont.Load(defaultFontStream);

            LOGGER.Info($"默认字体数据完成，字符数量:{_DefaultFont.Count} 字体高度:{_DefaultFont.Height} 全角宽度:{_DefaultFont.FullWidth} 半角宽度:{_DefaultFont.HalfWidth}");
        }

        private static void LoadCursorFile(ResourceEntryManager resources)
        {
            LOGGER.Info("开始加载光标文件");

            _CursorStyleManager = CursorStyleManager.LoadInstance();

            LOGGER.Info($"光标数据加载完成，光标数量:{_CursorStyleManager.Count}");
        }
    }
}
