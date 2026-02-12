using MCBS.Cursor.Style;
using QuanLib.BDF;
using QuanLib.Core;
using QuanLib.Logging;
using QuanLib.Minecraft.Versions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static MCBS.Config.ConfigManager;

namespace MCBS
{
    public static class SR
    {
        private static readonly ILogger LOGGER = Log4NetManager.Instance.GetLogger();

        public static bool IsLoaded { get; private set; }

        public static VersionList MinecraftVersionList
        {
            get => GetNotNull(field);
            private set => field = value;
        }

        public static MinecraftVersion CurrentMinecraftVersion
        {
            get => GetNotNull(field);
            private set => field = value;
        }

        public static BdfFont DefaultFont
        {
            get => GetNotNull(field);
            private set => field = value;
        }

        public static CursorStyleManager CursorStyleManager
        {
            get => GetNotNull(field);
            private set => field = value;
        }

        public static void LoadAll()
        {
            if (IsLoaded)
                throw new InvalidOperationException("系统资源管理器完成已初始化，无法重复初始化");
            IsLoaded = true;

            LoadMinecraftVersionList();
            LoadFontFile();
            LoadCursorFile();
        }

        private static void LoadMinecraftVersionList()
        {
            MinecraftVersionList = VersionList.LoadInstance(InstantiateArgs.Empty);
            CurrentMinecraftVersion = MinecraftVersionList.GetVersion(MinecraftConfig.MinecraftVersion);

            LOGGER.Info("Minecraft版本数据加载完成");
        }

        private static void LoadFontFile()
        {
            using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("MCBS.SystemResource.DefaultFont.bdf") ?? throw new InvalidOperationException();
            DefaultFont = BdfFont.Load(stream);

            LOGGER.Info($"BDF字体数据完成，字符数量:{DefaultFont.Count} 字符高度:{DefaultFont.Height} 全角宽度:{DefaultFont.FullWidth} 半角宽度:{DefaultFont.HalfWidth}");
        }

        private static void LoadCursorFile()
        {
            CursorStyleManager = CursorStyleManager.LoadInstance();

            LOGGER.Info($"光标数据加载完成，光标数量:{CursorStyleManager.Count}");
        }

        private static T GetNotNull<T>(T? field, [CallerMemberName] string? propertyName = null) where T : class
        {
            return field ?? throw new InvalidOperationException($"属性“{propertyName}”初始化");
        }
    }
}
