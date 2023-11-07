using log4net.Core;
using MCBS.Application;
using MCBS.Config;
using MCBS.Logging;
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
        private static readonly LogImpl LOGGER = LogUtil.GetLogger();

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
            LOGGER.Info("开始加载系统应用程序组件");

            List<ApplicationManifest> result = new();
            foreach (string dll in ConfigManager.SystemConfig.SystemAppComponents)
            {
                try
                {
                    Assembly assembly = Assembly.Load(dll);
                    ApplicationManifest applicationManifest = ApplicationManifestReader.Load(assembly);
                    result.Add(applicationManifest);
                }
                catch (Exception ex)
                {
                    LOGGER.Error($"无法加载位于“{dll}”的DLL应用程序组件", ex);
                }
            }

            LOGGER.Info($"完成，共加载{result.Count}个应用程序组件");

            return result.ToArray();
        }

        private static ApplicationManifest[] LoadDllAppComponents()
        {
            LOGGER.Info("开始加载DLL应用程序组件");

            List<ApplicationManifest> result = new();
            string[] dlls = SR.McbsDirectory.DllAppComponentsDir.GetFiles("*.dll");
            foreach (string dll in dlls)
            {
                try
                {
                    ApplicationManifest applicationManifest = ApplicationManifestReader.Load(dll);
                    result.Add(applicationManifest);
                }
                catch (Exception ex)
                {
                    LOGGER.Error($"无法加载位于“{dll}”的DLL应用程序组件", ex);
                }
            }

            LOGGER.Info($"完成，共加载{result.Count}个应用程序组件");

            return result.ToArray();
        }
    }
}
