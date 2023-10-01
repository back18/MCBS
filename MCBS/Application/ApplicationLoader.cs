using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Application
{
    public static class ApplicationLoader
    {
        public static ApplicationInfo Load(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException($"“{nameof(path)}”不能为 null 或空。", nameof(path));

            Assembly assembly = Assembly.LoadFrom(path);
            return Load(assembly);
        }

        public static ApplicationInfo Load(Assembly assembly)
        {
            if (assembly is null)
                throw new ArgumentNullException(nameof(assembly));

            Type attributeType = typeof(ApplicationInfoAttribute);
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsDefined(attributeType, false))
                    return Load(type);
            }

            throw new ArgumentException("在程序集中找不到主类", nameof(assembly));
        }

        public static ApplicationInfo Load(Type type)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));
            if (!type.IsSubclassOf(typeof(ApplicationInfo)))
                throw new ArgumentException($"“{nameof(type)}”必须继承自 {nameof(ApplicationInfo)}");
            if (type.IsAbstract)
                throw new ArgumentException($"“{nameof(type)}”不能为抽象类型");

            var constructor = type.GetConstructor(Type.EmptyTypes);
            if (constructor is null)
                throw new ArgumentException($"“{nameof(type)}”必须有一个无参构造函数");
            else if (!constructor.IsPublic)
                throw new ArgumentException($"“{nameof(type)}”的构造函数不是公共的");

            try
            {
                var result = constructor.Invoke(null);
                return (ApplicationInfo)result;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("无法创建应用程序信息实例", ex);
            }
        }
    }
}
