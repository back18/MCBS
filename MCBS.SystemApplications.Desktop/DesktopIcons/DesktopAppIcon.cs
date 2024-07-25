using MCBS.Application;
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

        public DesktopAppIcon(ApplicationManifest applicationManifest)
        {
            ArgumentNullException.ThrowIfNull(applicationManifest, nameof(applicationManifest));

            _applicationManifest = applicationManifest;
        }

        private readonly ApplicationManifest _applicationManifest;

        public override IconIdentifier GetIconIdentifier()
        {
            return new(ICON_TYPE, _applicationManifest.ID);
        }

        internal override string GetDisplayName()
        {
            return _applicationManifest.Name;
        }

        internal override Image<Rgba32> GetIncnImage()
        {
            return _applicationManifest.GetIcon();
        }

        internal override void OpenIcon()
        {
            MinecraftBlockScreen.Instance.ProcessManager.StartProcess(_applicationManifest, GetForm());
        }
    }
}
