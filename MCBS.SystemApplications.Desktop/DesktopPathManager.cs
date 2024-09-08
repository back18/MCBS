using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.SystemApplications.Desktop
{
    public class DesktopPathManager
    {
        public DesktopPathManager(string basePath)
        {
            ArgumentException.ThrowIfNullOrEmpty(basePath, nameof(basePath));

            ScreenDataDir = basePath;
            DesktopDir = Path.Combine(basePath, "Desktop");
            WallpapersDir = Path.Combine(basePath, "Wallpapers");
            IconDataFile = Path.Combine(basePath, "IconData.json");
        }

        public string ScreenDataDir { get; }

        public string DesktopDir { get; }

        public string WallpapersDir { get; }

        public string IconDataFile { get; }
    }
}
