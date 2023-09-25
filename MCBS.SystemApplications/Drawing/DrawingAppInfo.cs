using MCBS;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.SystemApplications.Drawing
{
    public class DrawingAppInfo : ApplicationInfo<DrawingApp>
    {
        public DrawingAppInfo()
        {
            Platforms = new PlatformID[]
            {
                PlatformID.Win32NT,
                PlatformID.Unix,
                PlatformID.MacOSX
            };
            ID = DrawingApp.ID;
            Name = DrawingApp.Name;
            Version = Version.Parse("1.0");
            try
            {
                Icon = Image.Load<Rgba32>(Path.Combine(MCOS.MainDirectory.Applications.GetApplicationDirectory(ID), "Icon.png"));
            }
            catch
            {
                Icon = GetDefaultIcon();
            }
            AppendToDesktop = true;
        }

        public override PlatformID[] Platforms { get; }

        public override string ID { get; }

        public override string Name { get; }

        public override Version Version { get; }

        public override bool AppendToDesktop { get; }

        protected override Image<Rgba32> Icon { get; }
    }
}
