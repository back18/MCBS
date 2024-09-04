using MCBS.Application;
using MCBS.BlockForms;
using MCBS.BlockForms.DialogBox;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.SystemApplications.Desktop.DesktopIcons
{
    public class DesktopAppIcon : DesktopIcon
    {
        public const string ICON_TYPE = "APP";

        public DesktopAppIcon(string appId)
        {
            ArgumentException.ThrowIfNullOrEmpty(appId, nameof(appId));

            _appId = appId;
        }

        private readonly string _appId;

        public override IconIdentifier GetIconIdentifier()
        {
            return new(ICON_TYPE, _appId);
        }

        internal override string GetDisplayName()
        {
            if (MinecraftBlockScreen.Instance.AppComponents.TryGetValue(_appId, out var applicationManifest))
                return applicationManifest.Name;
            else
                return _appId;
        }

        internal override Image<Rgba32> GetIncnImage()
        {
            if (MinecraftBlockScreen.Instance.AppComponents.TryGetValue(_appId, out var applicationManifest))
                return applicationManifest.GetIcon();
            else
                return ApplicationManifest.GetDefaultIcon();
        }

        internal override void OpenIcon()
        {
            Form? form = GetForm();
            if (form is null)
                return;

            if (!MinecraftBlockScreen.Instance.AppComponents.TryGetValue(_appId, out var applicationManifest))
            {
                _ = DialogBoxHelper.OpenMessageBoxAsync(form,
                    "错误",
                    $"找不到应用程序组件“{_appId}”",
                    MessageBoxButtons.OK);

                return;
            }

            MinecraftBlockScreen.Instance.ProcessManager.StartProcess(applicationManifest, form);
        }
    }
}
