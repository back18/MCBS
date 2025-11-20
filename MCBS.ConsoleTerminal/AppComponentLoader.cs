using MCBS.Application;
using MCBS.Config;
using QuanLib.Core;
using QuanLib.IO.Extensions;
using QuanLib.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.ConsoleTerminal
{
    public static class AppComponentLoader
    {
        private static readonly ILogger LOGGER = Log4NetManager.Instance.GetLogger();

        public static ApplicationManifest[] LoadAll()
        {
            List<ApplicationManifest> result = new();
            result.AddRange(LoadSystemAppComponents());
            if (ConfigManager.SystemConfig.LoadDllAppComponents)
                result.AddRange(LoadDllAppComponents());

            return result.ToArray();
        }

        private static ApplicationManifest[] LoadSystemAppComponents()
        {
            List<ApplicationManifest> result = new();
            foreach (string component in ConfigManager.SystemConfig.SystemAppComponents)
            {
                try
                {
                    Assembly assembly = Assembly.Load(component);
                    ApplicationManifest applicationManifest = ApplicationManifestReader.Load(assembly);
                    result.Add(applicationManifest);
                    LOGGER.Info($"成功从命名空间“{component}”加载应用程序组件“{applicationManifest.ID}-{applicationManifest.Version}”");
                }
                catch (Exception ex)
                {
                    LOGGER.Error($"无法从命名空间“{component}”加载应用程序组件", ex);
                }
            }

            return result.ToArray();
        }

        private static ApplicationManifest[] LoadDllAppComponents()
        {
            List<ApplicationManifest> result = new();
            string[] files = McbsPathManager.MCBS_DllAppComponents.GetFilePaths("*.dll");
            foreach (string file in files)
            {
                try
                {
                    ApplicationManifest applicationManifest = ApplicationManifestReader.Load(file);
                    result.Add(applicationManifest);
                    LOGGER.Info($"成功从文件“{Path.GetFileName(file)}”加载应用程序组件“{applicationManifest.ID}-{applicationManifest.Version}”");
                }
                catch (Exception ex)
                {
                    LOGGER.Error($"无法从文件“{Path.GetFileName(file)}”加载应用程序组件", ex);
                }
            }

            return result.ToArray();
        }
    }
}
