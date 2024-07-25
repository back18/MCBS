using MCBS.BlockForms.Utility;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuanLib.Core;

namespace MCBS.SystemApplications.Desktop.DesktopIcons
{
    public class DesktopDirectoryIcon : DesktopIcon
    {
        public const string ICON_TYPE = "DIRECTORY";

        public DesktopDirectoryIcon(string path)
        {
            ArgumentException.ThrowIfNullOrEmpty(nameof(path), path);

            _path = path;
        }

        private readonly string _path;

        public override IconIdentifier GetIconIdentifier()
        {
            return new(ICON_TYPE, Path.GetFileName(_path));
        }

        internal override string GetDisplayName()
        {
            return Path.GetFileName(_path);
        }

        internal override Image<Rgba32> GetIncnImage()
        {
            return PathIconManager.GetIcon(_path);
        }

        internal override void OpenIcon()
        {
            if (!Directory.Exists(_path))
                return;

            MinecraftBlockScreen.Instance.ProcessManager.StartProcess(MinecraftBlockScreen.Instance.AppComponents["System.FileExplorer"], [_path], GetForm());
        }
    }
}
