using MCBS.BlockForms.SimpleFileSystem;
using Nett;
using QuanLib.IO.Extensions;
using QuanLib.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.SystemApplications.FileExplorer.Config
{
    public class FileExplorerConfig
    {
        public FileExplorerConfig(Model model)
        {
            ArgumentNullException.ThrowIfNull(model, nameof(model));

            RootDirectory = model.RootDirectory;
        }

        public string RootDirectory { get; }

        public static void CreateIfNotExists()
        {
            DirectoryInfo directoryInfo = McbsPathManager.MCBS_Applications.CombineDirectory(FileExplorerApp.ID);
            string path = directoryInfo.CombineFile("Config.toml").FullName;
            directoryInfo.CreateIfNotExists();
            CreateIfNotExists(path, "MCBS.SystemApplications.FileExplorer.Config.Default.Config.toml");
        }

        private static void CreateIfNotExists(string path, string resource)
        {
            ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));
            ArgumentException.ThrowIfNullOrEmpty(resource, nameof(resource));

            if (!File.Exists(path))
            {
                using Stream? stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource) ?? throw new InvalidOperationException();
                using FileStream fileStream = new(path, FileMode.Create);
                stream.CopyTo(fileStream);
                LogManager.Instance.GetLogger().Warn($"配置文件“{path}”不存在，已创建默认配置文件");
            }
        }

        public static FileExplorerConfig Load()
        {
            string path = McbsPathManager.MCBS_Applications.CombineFile(FileExplorerApp.ID, "Config.toml").FullName;
            TomlTable table = Toml.ReadFile(path);
            Model model = table.Get<Model>();

            model.RootDirectory = SimpleFilesBox.GetFullPath(model.RootDirectory);
            if (!Directory.Exists(model.RootDirectory))
                model.RootDirectory = string.Empty;

            return new(model);
        }

        public class Model
        {
            public required string RootDirectory { get; set; }
        }
    }
}
