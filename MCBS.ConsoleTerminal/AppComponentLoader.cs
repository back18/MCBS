using log4net.Core;
using MCBS.Application;
using MCBS.Config;
using MCBS.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.ConsoleTerminal
{
    public static class AppComponentLoader
    {
        private static readonly LogImpl LOGGER = LogUtil.GetLogger();

        public static void LoadAll(MCOS mcos)
        {
            if (mcos is null)
                throw new ArgumentNullException(nameof(mcos));

            LoadSystemAppComponents(mcos);
            if (ConfigManager.SystemConfig.LoadDllAppComponents)
                LoadDllAppComponents(mcos);
        }

        private static void LoadSystemAppComponents(MCOS mcos)
        {
            LOGGER.Info("开始加载系统应用程序");

            if (mcos is null)
                throw new ArgumentNullException(nameof(mcos));

            mcos.ApplicationManager.Items.Add(new SystemApplications.Services.ServicesAppInfo());
            mcos.ApplicationManager.Items.Add(new SystemApplications.Desktop.DesktopAppInfo());
            mcos.ApplicationManager.Items.Add(new SystemApplications.Settings.SettingsAppInfo());
            mcos.ApplicationManager.Items.Add(new SystemApplications.ScreenManager.ScreenManagerAppInfo());
            mcos.ApplicationManager.Items.Add(new SystemApplications.TaskManager.TaskManagerAppInfo());
            mcos.ApplicationManager.Items.Add(new SystemApplications.FileExplorer.FileExplorerAppInfo());
            mcos.ApplicationManager.Items.Add(new SystemApplications.Notepad.NotepadAppInfo());
            mcos.ApplicationManager.Items.Add(new SystemApplications.Album.AlbumAppInfo());
            mcos.ApplicationManager.Items.Add(new SystemApplications.Drawing.DrawingAppInfo());
            mcos.ApplicationManager.Items.Add(new SystemApplications.VideoPlayer.VideoPlayerAppInfo());
            mcos.ApplicationManager.Items.Add(new SystemApplications.DataScreen.DataScreenAppInfo());

            LOGGER.Info("完成");
        }

        private static void LoadDllAppComponents(MCOS mcos)
        {
            LOGGER.Info("开始加载DLL应用程序");

            if (mcos is null)
                throw new ArgumentNullException(nameof(mcos));

            string[] dlls = SR.McbsDirectory.DllAppComponentsDir.GetFiles("*.dll");
            foreach (string dll in dlls)
            {
                try
                {
                    ApplicationInfo applicationInfo = Application.ApplicationLoader.Load(dll);
                    mcos.ApplicationManager.Items.Add(applicationInfo);
                }
                catch (Exception ex)
                {
                    LOGGER.Error($"无法加载位于“{dll}”的应用程序", ex);
                }
            }

            LOGGER.Info("完成");
        }
    }
}
