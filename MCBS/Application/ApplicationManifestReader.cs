using Newtonsoft.Json;
using QuanLib.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Application
{
    public class ApplicationManifestReader
    {
        public static ApplicationManifest Load(string path)
        {
            ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));

            Assembly assembly = Assembly.LoadFrom(path);
            return Load(assembly);
        }

        public static ApplicationManifest Load(Assembly assembly)
        {
            ArgumentNullException.ThrowIfNull(assembly, nameof(assembly));

            string path = assembly.GetName().Name + ".McbsApplication.json";
            Stream? stream = assembly.GetManifestResourceStream(path) ?? throw new InvalidOperationException("找不到配置文件");
            try
            {
                ApplicationManifest.Model model = JsonConvert.DeserializeObject<ApplicationManifest.Model>(stream.ReadAllText()) ?? throw new NullReferenceException();
                return new(assembly, model);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("无法解析配置文件", ex);
            }
        }
    }
}
