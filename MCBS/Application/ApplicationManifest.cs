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
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"MCBS.SystemResource.DefaultIcon.png") ?? throw new InvalidOperationException();
            _defaultIcon = Image.Load<Rgba32>(stream);
        }

        public ApplicationManifest(Assembly assembly, Model model)
        {
            ArgumentNullException.ThrowIfNull(assembly, nameof(assembly));
            NullValidator.ValidateObject(model, nameof(model));

            Type? type = assembly.GetType(model.MainClass) ?? throw new InvalidOperationException("找不到主类");
            if (!typeof(IProgram).IsAssignableFrom(type))
                throw new InvalidOperationException($"主类未实现 {nameof(IProgram)} 接口");
            if (type.IsAbstract)
                throw new InvalidOperationException($"主类不能为抽象类型");

            var constructor = type.GetConstructor(Type.EmptyTypes) ?? throw new InvalidOperationException($"找不到主类构造函数");
            if (!constructor.IsPublic)
                throw new InvalidOperationException($"主类构造函数无法访问");

            Assembly = assembly;
            MainClass = type;
            ID = model.ID;
            Name = model.Name;
            Version = Version.Parse(model.Version);
            IsBackground = model.IsBackground;

            try
            {
                Stream? stream = assembly.GetManifestResourceStream(model.Icon);
                if (stream is not null)
                    Icon = Image.Load<Rgba32>(stream);
                else
                    Icon = GetDefaultIcon();

            }
            catch
            {
                Icon = GetDefaultIcon();
            }

            Environment = Platforms.None;
            foreach (string platform in model.Environment)
            {
                switch (platform)
                {
                    case "WINDOWS":
                        Environment |= Platforms.Windows;
                        break;
                    case "LINUX":
                        Environment |= Platforms.Linux;
                        break;
                    case "MACOS":
                        Environment |= Platforms.MacOS;
                        break;
                    default:
                        break;
                }
            }

            List<ApplicationDependencie> descriptions = [];
            foreach (ApplicationDependencie.Model description in model.Dependencies)
                descriptions.Add(new(description));

            Dependencies = new(descriptions);
            Authors = new(model.Authors);
        }

        private readonly static Image<Rgba32> _defaultIcon;

        public Assembly Assembly { get; }

        public Type MainClass { get; }

        public string ID { get; }

        public string Name { get; }

        public Version Version { get; }

        public bool IsBackground { get; }

        private Image<Rgba32> Icon { get; }

        public Platforms Environment { get; }

        public ReadOnlyCollection<ApplicationDependencie> Dependencies { get; }

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

        public override string ToString()
        {
            return $"{Name} - {ID} ({Version})";
        }

        public class Model
        {
            public required string MainClass { get; set; }

            public required string ID { get; set; }

            public required string Name { get; set; }

            public required string Version { get; set; }

            public required string Icon { get; set; }

            public required bool IsBackground { get; set; }

            public required string[] Environment { get; set; }

            public required ApplicationDependencie.Model[] Dependencies { get; set; }

            public required string[] Authors { get; set; }
        }
    }
}
