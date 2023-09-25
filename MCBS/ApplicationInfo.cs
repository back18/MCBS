using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MCBS
{
    public abstract class ApplicationInfo
    {
        static ApplicationInfo()
        {
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(SR.SYSTEM_RESOURCE_NAMESPACE + ".DefaultIcon.png") ?? throw new InvalidOperationException();
            _defaultIcon = Image.Load<Rgba32>(stream);
        }

        protected ApplicationInfo(Type typeObject)
        {
            if (typeObject is null)
                throw new ArgumentNullException(nameof(typeObject));
            if (!typeObject.IsAssignableFrom(typeof(Application)))
                throw new ArgumentException($"“{nameof(typeObject)}”必须继承自 {nameof(Application)}");
            if (typeObject.IsAbstract)
                throw new ArgumentException($"“{nameof(typeObject)}”不能为抽象类型");
            if (typeObject.GetConstructor(Type.EmptyTypes) is null)
                throw new ArgumentException($"“{nameof(typeObject)}”必须有一个无参构造函数");

            TypeObject = typeObject;
        }

        private readonly static Image<Rgba32> _defaultIcon;

        public Type TypeObject { get; }

        public abstract PlatformID[] Platforms { get; }

        public abstract string ID { get; }

        public abstract string Name { get; }

        public abstract Version Version { get; }

        public abstract bool AppendToDesktop { get; }

        protected abstract Image<Rgba32> Icon { get; }

        public virtual string ApplicationDirectory => MCOS.MainDirectory.Applications.GetApplicationDirectory(ID);

        public virtual Image<Rgba32> GetIcon() => Icon.Clone();

        public static Image<Rgba32> GetDefaultIcon() => _defaultIcon.Clone();

        public virtual Application CreateApplicationInstance()
        {
            var instance = Activator.CreateInstance(TypeObject);
            if (instance is not Application application)
                throw new InvalidOperationException("无法创建应用程序实例");

            return application;
        }

        public override string ToString()
        {
            return $"Name={Name}, ID={ID}, Version={Version}";
        }
    }

    public abstract class ApplicationInfo<T> : ApplicationInfo where T : Application
    {
        protected ApplicationInfo() : base(typeof(T)) { }
    }
}
