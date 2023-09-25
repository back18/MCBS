using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.SystemApplications.SystemBoot
{
    public class SystemBootAppInfo : ApplicationInfo<SystemBootApp>
    {
        public SystemBootAppInfo()
        {
            Platforms = new PlatformID[]
            {
                PlatformID.Win32NT,
                PlatformID.Unix,
                PlatformID.MacOSX
            };
            ID = SystemBootApp.ID;
            Name = SystemBootApp.Name;
            Version = Version.Parse("1.0");
            Icon = GetDefaultIcon();
            AppendToDesktop = false;
        }

        public override PlatformID[] Platforms { get; }

        public override string ID { get; }

        public override string Name { get; }

        public override Version Version { get; }

        public override bool AppendToDesktop { get; }

        protected override Image<Rgba32> Icon { get; }
    }
}
