using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.SystemApplications.Desktop.DesktopIcons
{
    public static class DesktopIconFactory
    {
        public static DesktopIcon CreateDesktopIcon(string desktopDirectory, IconIdentifier iconIdentifier)
        {
            ArgumentException.ThrowIfNullOrEmpty(desktopDirectory, nameof(desktopDirectory));

            return iconIdentifier.Type switch
            {
                DesktopAppIcon.ICON_TYPE => new DesktopAppIcon(MinecraftBlockScreen.Instance.AppComponents[iconIdentifier.Value]),
                DesktopFileIcon.ICON_TYPE => new DesktopFileIcon(Path.Combine(desktopDirectory, iconIdentifier.Value)),
                DesktopDirectoryIcon.ICON_TYPE => new DesktopDirectoryIcon(Path.Combine(desktopDirectory, iconIdentifier.Value)),
                _ => throw new InvalidOperationException("未知的图标类型：" + iconIdentifier.Value),
            };
        }
    }
}
