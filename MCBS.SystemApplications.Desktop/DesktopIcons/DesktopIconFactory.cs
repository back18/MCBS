using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.SystemApplications.Desktop.DesktopIcons
{
    public static class DesktopIconFactory
    {
        public static DesktopIcon CreateDesktopIcon(string desktopPath, IconIdentifier iconIdentifier)
        {
            ArgumentException.ThrowIfNullOrEmpty(desktopPath, nameof(desktopPath));

            return iconIdentifier.Type switch
            {
                DesktopAppIcon.ICON_TYPE => new DesktopAppIcon(iconIdentifier.Value),
                DesktopFileIcon.ICON_TYPE => new DesktopFileIcon(Path.Combine(desktopPath, iconIdentifier.Value)),
                DesktopDirectoryIcon.ICON_TYPE => new DesktopDirectoryIcon(Path.Combine(desktopPath, iconIdentifier.Value)),
                _ => throw new InvalidOperationException("未知的图标类型：" + iconIdentifier.Type),
            };
        }
    }
}
