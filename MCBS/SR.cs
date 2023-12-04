using static MCBS.Config.ConfigManager;
using log4net.Core;
using MCBS.Cursor.Style;
using MCBS.Directorys;
using MCBS.Logging;
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
using SixLabors.ImageSharp.PixelFormats;
using Newtonsoft.Json;
using MCBS.Application;

namespace MCBS
{
    public static class SR
    {
        private static LogImpl LOGGER => LogUtil.GetLogger();

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
            LOGGER.Info("开始加载Minecraft方块纹理");

            _BlockTextureManager = BlockTextureManager.LoadInstance(resources, MinecraftConfig.BlockTextureBlacklist);
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

            LOGGER.Info("完成，方块数量: " + _BlockTextureManager.Count);
        }

        private static void BuildColorMappingCache(ResourceEntryManager resources)
        {
            LOGGER.Info("开始构建Minecraft方块颜色映射表缓存");

            Dictionary<Facing, ColorMappingCache> caches = new();
            foreach (Facing facing in SystemConfig.BuildColorMappingCaches)
            {
                ColorMappingCache cache = ColorMappingCacheBuilder.Build(facing);
                caches.Add(facing, cache);
            }
            _ColorMappingCaches = new(caches);

            LOGGER.Info("完成");
        }

        private static void LoadMinecraftLanguage(ResourceEntryManager resources)
        {
            LOGGER.Info("开始加载Minecraft语言文件，语言标识: " + MinecraftConfig.Language);

            VersionDirectory versionDirectory = SR.McbsDirectory.MinecraftDir.VanillaDir.GetVersionDirectory(MinecraftConfig.GameVersion);
            string? minecraftLanguageFilePath = versionDirectory.LanguagesDir.Combine(MinecraftConfig.Language + ".json");
            if (!File.Exists(minecraftLanguageFilePath))
                minecraftLanguageFilePath = null;
            _LanguageManager = LanguageManager.LoadInstance(resources, MinecraftConfig.Language, minecraftLanguageFilePath);

            LOGGER.Info("完成，语言条目数量: " + _LanguageManager.Count);
        }

        private static void LoadFontFile(ResourceEntryManager resources)
        {
            LOGGER.Info("开始加载字体文件");

            using Stream defaultFontStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(SystemResourceNamespace.DefaultFontFile) ?? throw new InvalidOperationException();
            _DefaultFont = BdfFont.Load(defaultFontStream);

            LOGGER.Info($"完成，字体高度:{_DefaultFont.Height} 半角宽度:{_DefaultFont.HalfWidth} 全角宽度:{_DefaultFont.FullWidth} 字符数量:{_DefaultFont.Count}");
        }

        private static void LoadCursorFile(ResourceEntryManager resources)
        {
            LOGGER.Info("开始加载光标文件");

            _CursorStyleManager = CursorStyleManager.LoadInstance();

            LOGGER.Info("完成，光标样式数量: " + _CursorStyleManager.Count);
        }
    }
}
