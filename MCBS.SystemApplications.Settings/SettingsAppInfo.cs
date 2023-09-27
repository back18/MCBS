using MCBS.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.SystemApplications.Settings
{
    public class SettingsAppInfo : ApplicationInfo<SettingsApp>
    {
        public SettingsAppInfo()
        {
            ID = SettingsApp.ID;
            Name = SettingsApp.Name;
            Version = Version.Parse("1.0");
            AppendToDesktop = true;
            Platforms = new PlatformID[]
            {
                PlatformID.Win32NT,
                PlatformID.Unix,
                PlatformID.MacOSX
            };
            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                using Stream stream = assembly.GetManifestResourceStream(assembly.GetName().Name + ".SystemResource.Icon.png") ?? throw new InvalidOperationException();
                Icon = Image.Load<Rgba32>(stream);
            }
            catch (Exception ex)
            {
                Icon = GetDefaultIcon();
                LogUtil.GetLogger().Warn($"无法获取应用程序“{ID}”的图标，已应用默认图标", ex);
            }
        }

        public override string ID { get; }

        public override string Name { get; }

        public override Version Version { get; }

        public override bool AppendToDesktop { get; }

        protected override PlatformID[] Platforms { get; }

        protected override Image<Rgba32> Icon { get; }
    }
}
