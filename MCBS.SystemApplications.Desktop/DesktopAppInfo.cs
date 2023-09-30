using MCBS.Application;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.SystemApplications.Desktop
{
    [ApplicationInfo]
    public class DesktopAppInfo : ApplicationInfo<DesktopApp>
    {
        public DesktopAppInfo()
        {
            ID = DesktopApp.ID;
            Name = DesktopApp.Name;
            Version = Version.Parse("1.0");
            AppendToDesktop = false;
            Platforms = new PlatformID[]
            {
                PlatformID.Win32NT,
                PlatformID.Unix,
                PlatformID.MacOSX
            };
            Icon = GetDefaultIcon();
        }

        public override string ID { get; }

        public override string Name { get; }

        public override Version Version { get; }

        public override bool AppendToDesktop { get; }

        protected override PlatformID[] Platforms { get; }

        protected override Image<Rgba32> Icon { get; }
    }
}
