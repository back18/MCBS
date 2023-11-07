using MCBS.Logging;
using QuanLib.Core;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Application
{
    public class ApplicationManifest
    {
        static ApplicationManifest()
        {
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(SR.SystemResourceNamespace.DefaultIconFile) ?? throw new InvalidOperationException();
            _defaultIcon = Image.Load<Rgba32>(stream);
        }

        public ApplicationManifest(Assembly assembly, Model model)
        {
            if (assembly is null)
                throw new ArgumentNullException(nameof(assembly));
            NullValidator.ValidateObject(model, nameof(model));

            Type? type = assembly.GetType(model.MainClass);
            if (type is null)
                throw new InvalidOperationException("找不到主类");
            if (!typeof(IProgram).IsAssignableFrom(type))
                throw new InvalidOperationException($"主类未实现 {nameof(IProgram)} 接口");
            if (type.IsAbstract)
                throw new InvalidOperationException($"主类不能为抽象类型");

            var constructor = type.GetConstructor(Type.EmptyTypes);
            if (constructor is null)
                throw new InvalidOperationException($"找不到主类构造函数");
            if (!constructor.IsPublic)
                throw new InvalidOperationException($"主类构造函数无法访问");

            Assembly = assembly;
            MainClass = type;
            ID = model.ID;
            Name = model.Name;
            Version = Version.Parse(model.Version);

            Environment = PlatformType.None;
            foreach (string platform in model.Environment)
            {
                switch (platform)
                {
                    case "WINDOWS":
                        Environment |= PlatformType.Windows;
                        break;
                    case "LINUX":
                        Environment |= PlatformType.Linux;
                        break;
                    case "MACOS":
                        Environment |= PlatformType.MacOS;
                        break;
                    default:
                        break;
                }
            }

            Stream? stream = assembly.GetManifestResourceStream(model.Icon);
            if (stream is null)
            {
                Icon = GetDefaultIcon();
                LogUtil.GetLogger().Warn($"找不到应用程序“{ID}”位于路径“{model.Icon}”的图标，已应用默认图标");
            }
            else
            {
                try
                {
                    Icon = Image.Load<Rgba32>(stream);
                }
                catch (Exception ex)
                {
                    Icon = GetDefaultIcon();
                    LogUtil.GetLogger().Warn($"应用程序“{ID}”位于路径“{model.Icon}”的图标无法加载，已应用默认图标", ex);
                }
            }

            List<ApplicationDescription> descriptions = new();
            foreach (ApplicationDescription.Model description in model.Descriptions)
                descriptions.Add(new(description));

            Descriptions = new(descriptions);
            Authors = new(model.Authors);
        }

        private readonly static Image<Rgba32> _defaultIcon;

        public Assembly Assembly { get; }

        public Type MainClass { get; }

        public string ID { get; }

        public string Name { get; }

        public Version Version { get; }

        private Image<Rgba32> Icon { get; }

        public PlatformType Environment { get; }

        public ReadOnlyCollection<ApplicationDescription> Descriptions { get; }

        public ReadOnlyCollection<string> Authors { get; }

        public Image<Rgba32> GetIcon() => Icon.Clone();

        public static Image<Rgba32> GetDefaultIcon() => _defaultIcon.Clone();

        public IProgram CreateApplicationInstance()
        {
            var instance = Activator.CreateInstance(MainClass);
            if (instance is not IProgram program)
                throw new InvalidOperationException("无法创建应用程序主类实例");

            return program;
        }

        public class Model
        {
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
            public string MainClass { get; set; }

            public string ID { get; set; }

            public string Name { get; set; }

            public string Version { get; set; }

            public string Icon { get; set; }

            public string[] Environment { get; set; }

            public ApplicationDescription.Model[] Descriptions { get; set; }

            public string[] Authors { get; set; }
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        }
    }
}
