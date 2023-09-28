using FFMediaToolkit;
using MCBS.BlockForms.DialogBox;
using MCBS.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using MCBS.Logging;
using System.Reflection;

namespace MCBS.SystemApplications.PackageManager
{
    public class PackageManager : Application
    {
        public const string ID = "PackageManager";

        public const string Name = "包管理器";

        public override object? Main(string[] args)
        {
            string? path = null;
            if (args.Length > 0)
                path = args[0];

            RunForm(new PackageManagerForm(path));
            return null;
        }

        public class PackageList
        {
            public List<Package> packages = new List<Package>();

            public class Package
            {
                public string name = "";
                public string author = "";
                public string repo = "";
            }
        }
        public static async Task<byte[]> GetWebsiteAsByteArray(string url)
        {
            try
            {
                HttpClient client = new HttpClient();
                HttpResponseMessage response = await client.GetAsync(url);
                return await response.Content.ReadAsByteArrayAsync();
            }
            catch (Exception ex)
            {
                return new byte[0];
            }
        }

        public static async Task<string> GetWebsiteAsText(string url)
        {
            try
            {
                HttpClient client = new HttpClient();
                HttpResponseMessage response = await client.GetAsync(url);
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                return "{\"packages\":[{\"name\":\"加载失败！\",\"author\":\"加载失败！\",\"repo\":\"null\"}]}";
            }
        }

        public async static Task<List<PackageList.Package>> _GetPKGMList()
        {
            string resp = await GetWebsiteAsText("http://github.com/SALTWOOD/pkgm-source/raw/main/pkg-list.json");
            PackageList pkgList = JsonConvert.DeserializeObject<PackageList>(resp)!;
            return pkgList.packages;
        }

        public static List<PackageList.Package> GetPKGMList()
        {
            Task<List<PackageList.Package>> task = _GetPKGMList();
            task.Wait();
            return task.Result;
        }

        public static List<object?> GetExternalApp(string path)
        {
            Assembly assembly = Assembly.LoadFrom(path);
            Type[] types = assembly.GetTypes();
            List<object?> objects = new List<object?>();

            foreach (Type type in types)
            {
                try
                {
                    bool isSubclass = type.IsSubclassOf(typeof(ApplicationInfo));
                    ApplicationInfoAttribute? attr = type.GetCustomAttribute<ApplicationInfoAttribute>();

                    if (isSubclass && attr != null)
                    {
                        object? instance = Activator.CreateInstance(type);
                        objects.Add(instance);
                    }
                }
                finally { }
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
