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
        private static readonly LoadedResult EmptyLoadedResult = new([], []);

        public static ApplicationManifest[] LoadAll()
        {
            List<ApplicationManifest> manifests = [];

            LOGGER.Info("开始加载系统内置应用程序组件...");
            LoadedResult systemAppComponent = LoadSystemAppComponents(ConfigManager.SystemConfig.SystemAppComponents);
            manifests.AddRange(systemAppComponent.Manifests);
            LOGGER.Info(FormatLogMessage(systemAppComponent));

            if (ConfigManager.SystemConfig.LoadDllAppComponents)
            {
                LOGGER.Info("开始加载外部DLL应用程序组件...");
                LoadedResult dllAppComponent = LoadDllAppComponents(McbsPathManager.MCBS_DllAppComponents.FullName);
                manifests.AddRange(dllAppComponent.Manifests);
                LOGGER.Info(FormatLogMessage(dllAppComponent));
            }
            else
            {
                LOGGER.Info("外部DLL应用程序组件被禁用，已跳过加载");
            }

            return manifests.ToArray();
        }

        private static LoadedResult LoadSystemAppComponents(IList<string> components)
        {
            ArgumentNullException.ThrowIfNull(components, nameof(components));

            if (components.Count == 0)
                return EmptyLoadedResult;

            List<ApplicationManifest> manifests = [];
            List<ApplicationLoadException> exceptions = [];

            foreach (string component in components)
            {
                try
                {
                    ApplicationManifest manifest = ApplicationLoader.LoadFromName(component);
                    manifests.Add(manifest);
                }
                catch (ApplicationLoadException ex)
                {
                    exceptions.Add(ex);
                }
            }

            return new(manifests.ToArray(), exceptions.ToArray());
        }

        private static LoadedResult LoadDllAppComponents(string directory)
        {
            if (!Directory.Exists(directory))
                return EmptyLoadedResult;

            string[] components = Directory.GetFiles("*.dll");
            if (components.Length == 0)
                return EmptyLoadedResult;

            List<ApplicationManifest> manifests = [];
            List<ApplicationLoadException> exceptions = [];

            foreach (string component in components)
            {
                try
                {
                    ApplicationManifest manifest = ApplicationLoader.LoadFromFile(component);
                    manifests.Add(manifest);
                }
                catch (ApplicationLoadException ex)
                {
                    exceptions.Add(ex);
                }
            }

            return new(manifests.ToArray(), exceptions.ToArray());
        }

        private static string FormatLogMessage(LoadedResult loadedResult)
        {
            ArgumentNullException.ThrowIfNull(loadedResult, nameof(loadedResult));

            if (loadedResult.Manifests.Length == 0 && loadedResult.Exceptions.Length == 0)
                return "未找到任何应用程序组件";

            StringBuilder stringBuilder = new();
            stringBuilder.Append($"已加载{loadedResult.Manifests.Length + loadedResult.Exceptions.Length}个应用程序组件");

            if (loadedResult.Exceptions.Length == 0)
                stringBuilder.AppendLine("，全部加载成功");
            else
                stringBuilder.AppendLine($"{loadedResult.Manifests.Length}个加载成功，{loadedResult.Exceptions.Length}个加载失败");

            foreach (ApplicationManifest manifest in loadedResult.Manifests)
                stringBuilder.AppendLine($"    - 成功加载 {manifest.Assembly.GetName().Name} -> {manifest.Id} - {manifest.Version}");

            foreach (ApplicationLoadException exception in loadedResult.Exceptions)
                stringBuilder.AppendLine($"    - 加载失败 {exception.Assembly} -> {exception.ApplicationLoadErrorCode} ({(int)exception.ApplicationLoadErrorCode})");

            stringBuilder.Length -= Environment.NewLine.Length;
            return stringBuilder.ToString();
        }

        private record LoadedResult(ApplicationManifest[] Manifests, ApplicationLoadException[] Exceptions);
    }
}
