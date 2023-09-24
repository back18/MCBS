using log4net.Core;
using MCBS.Logging;
using QuanLib.BDF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MCBS
{
    public static class SystemResourcesManager
    {
        private static readonly LogImpl LOGGER = LogUtil.MainLogger;

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

        public static CursorManager CursorManager
        {
            get
            {
                if (_CursorManager is null)
                    throw new InvalidOperationException();
                return _CursorManager;
            }
        }
        private static CursorManager? _CursorManager;

        public static void LoadAll()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();

            LOGGER.Info("开始加载默认字体资源文件");
            using Stream defaultFontStream = assembly.GetManifestResourceStream("MCBS.SystemResource.DefaultFont.bdf") ?? throw new IndexOutOfRangeException();
            _DefaultFont = BdfFont.Load(defaultFontStream);
            LOGGER.Info($"完成，字体高度:{DefaultFont.Height} 半角宽度:{DefaultFont.HalfWidth} 全角宽度:{DefaultFont.FullWidth} 字符数量:{DefaultFont.Count}");

            _CursorManager = CursorManager.Load(MCOS.MainDirectory.SystemResources.Cursors.FullPath);
            LOGGER.Info($"光标文件加载完成");
        }
    }
}
