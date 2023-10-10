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

        public static BlockTextureManager BlockTextureManager
        {
            get
            {
                if (_BlockTextureManager is null)
                    throw new InvalidOperationException();
                return _BlockTextureManager;
            }
        }
        private static BlockTextureManager? _BlockTextureManager;

        public static LanguageManager LanguageManager
        {
            get
            {
                if (_LanguageManager is null)
                    throw new InvalidOperationException();
                return _LanguageManager;
            }
        }
        private static LanguageManager? _LanguageManager;

        public static BdfFont DefaultFont
        {
            get
            {
                if (_DefaultFont is null)
                    throw new InvalidOperationException();
                return _DefaultFont;
            }
        }
        private static BdfFont? _DefaultFont;

        public static CursorStyleManager CursorStyleManager
        {
            get
            {
                if (_CursorStyleManager is null)
                    throw new InvalidOperationException();
                return _CursorStyleManager;
            }
        }
        private static CursorStyleManager? _CursorStyleManager;

        public static void LoadAll(ResourceEntryManager resources)
        {
            if (resources is null)
                throw new ArgumentNullException(nameof(resources));

            Assembly assembly = Assembly.GetExecutingAssembly();
            VersionDirectory versionDirectory = SR.McbsDirectory.MinecraftResourcesDir.VanillaDir.GetVersionDirectory(MinecraftConfig.GameVersion);

            LOGGER.Info("开始加载Minecraft方块纹理");
            _BlockTextureManager = BlockTextureManager.LoadInstance(resources, MinecraftConfig.BlockTextureBlacklist);
            LOGGER.Info("完成，方块数量: " + _BlockTextureManager.Count);

            LOGGER.Info("开始加载Minecraft语言文件，语言标识: " + MinecraftConfig.Language);
            string? minecraftLanguageFilePath = versionDirectory.LanguagesDir.Combine(MinecraftConfig.Language + ".json");
            if (!File.Exists(minecraftLanguageFilePath))
                minecraftLanguageFilePath = null;
            _LanguageManager = LanguageManager.LoadInstance(resources, MinecraftConfig.Language, minecraftLanguageFilePath);
            LOGGER.Info("完成，语言条目数量: " + _LanguageManager.Count);

            LOGGER.Info("开始加载默认字体资源文件");
            using Stream defaultFontStream = assembly.GetManifestResourceStream(SystemResourceNamespace.DefaultFontFile) ?? throw new InvalidOperationException();
            _DefaultFont = BdfFont.Load(defaultFontStream);
            LOGGER.Info($"完成，字体高度:{_DefaultFont.Height} 半角宽度:{_DefaultFont.HalfWidth} 全角宽度:{_DefaultFont.FullWidth} 字符数量:{_DefaultFont.Count}");

            LOGGER.Info("开始加载光标样式文件");
            _CursorStyleManager = CursorStyleManager.LoadInstance();
            LOGGER.Info("完成，光标样式数量: " + _CursorStyleManager.Count);
        }
    }
}
