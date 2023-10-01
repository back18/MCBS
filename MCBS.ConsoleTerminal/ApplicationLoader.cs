using MCBS.Application;
using MCBS.SystemApplications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.ConsoleTerminal
{
    [Obsolete("改用 MCBS.ConsoleTerminal.AppComponentLoader MCBS.Application.ApplicationLoader", true)]
    internal class ApplicationLoader
    {
        public static void LoadApplication(ref MCOS mcos, string path)
        {
            LoadInternalApp(ref mcos);

            // 尝试加载外部程序
            try
            {
                LoadExternalApp(ref mcos, path);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public static void LoadInternalApp(ref MCOS mcos)
        {
            mcos.ApplicationManager.Items.Add(new MCBS.SystemApplications.Services.ServicesAppInfo());
            mcos.ApplicationManager.Items.Add(new MCBS.SystemApplications.Desktop.DesktopAppInfo());
            mcos.ApplicationManager.Items.Add(new MCBS.SystemApplications.Settings.SettingsAppInfo());
            mcos.ApplicationManager.Items.Add(new MCBS.SystemApplications.ScreenManager.ScreenManagerAppInfo());
            mcos.ApplicationManager.Items.Add(new MCBS.SystemApplications.TaskManager.TaskManagerAppInfo());
            mcos.ApplicationManager.Items.Add(new MCBS.SystemApplications.FileExplorer.FileExplorerAppInfo());
            mcos.ApplicationManager.Items.Add(new MCBS.SystemApplications.Notepad.NotepadAppInfo());
            mcos.ApplicationManager.Items.Add(new MCBS.SystemApplications.Album.AlbumAppInfo());
            mcos.ApplicationManager.Items.Add(new MCBS.SystemApplications.Drawing.DrawingAppInfo());
            mcos.ApplicationManager.Items.Add(new MCBS.SystemApplications.VideoPlayer.VideoPlayerAppInfo());
            mcos.ApplicationManager.Items.Add(new MCBS.SystemApplications.DataScreen.DataScreenAppInfo());
        }

        public static void LoadExternalApp(ref MCOS mcos, string folderPath)
        {
            foreach (string dll in GetDllFiles(folderPath))
            {
                foreach (object? obj in GetExternalApp(dll))
                {
                    ApplicationInfo? app = obj as ApplicationInfo;
                    if (app != null)
                    {
                        mcos.ApplicationManager.Items.Add(app);
                    }
                }
            }
        }

        public static List<object?> GetExternalApp(string path)
        {
            Assembly assembly = Assembly.LoadFrom(path);
            Type[] types = assembly.GetTypes();
            List<object?> objects = new List<object?>();

            foreach (Type type in types)
            {
                bool isSubclass = type.IsSubclassOf(typeof(ApplicationInfo));

                if (isSubclass)
                {
                    object? instance = Activator.CreateInstance(type);
                    objects.Add(instance);
                }
            }
            return objects;
        }

        public static string[] GetDllFiles(string folderPath)
        {
            string[] dllFiles = Directory.GetFiles(folderPath, "*.dll");
            return dllFiles;
        }
    }
}
