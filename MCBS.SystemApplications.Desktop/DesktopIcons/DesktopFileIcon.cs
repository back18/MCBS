using MCBS.BlockForms;
using MCBS.BlockForms.DialogBox;
using MCBS.BlockForms.Utility;
using MCBS.Config;
using MCBS.UI;
using MCBS.UI.Extensions;
using QuanLib.Core;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.SystemApplications.Desktop.DesktopIcons
{
    public class DesktopFileIcon : DesktopIcon
    {
        public const string ICON_TYPE = "FILE";

        public DesktopFileIcon(string path)
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
            if (!File.Exists(_path))
                return;

            string extension = Path.GetExtension(_path).TrimStart('.');
            if (CoreConfigManager.Registry.TryGetValue(extension, out var appId) &&
                MinecraftBlockScreen.Instance.AppComponents.TryGetValue(appId, out var applicationManifest))
            {
                MinecraftBlockScreen.Instance.ProcessManager.StartProcess(applicationManifest, [_path], this.GetForm());
            }
            else
            {
                SelectApplication();
            }
        }

        private void SelectApplication()
        {
            IForm? form = this.GetForm();
            if (form is null)
                return;

            _ = DialogBoxHelper.OpenApplicationListBoxAsync(form, "请选择应用程序", (applicationManifest) =>
            {
                if (applicationManifest is not null)
                    MinecraftBlockScreen.Instance.ProcessManager.StartProcess(applicationManifest, [_path], form);
            });
        }
    }
}
