using Nett;
using Newtonsoft.Json;
using QuanLib.Core;
using QuanLib.DataAnnotations;
using QuanLib.Minecraft;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Config
{
    public class SystemConfig : IDataModelOwner<SystemConfig, SystemConfig.Model>
    {
        public SystemConfig(Model model)
        {
            NullValidator.ValidateObject(model, nameof(model));

            CrashAutoRestart = model.CrashAutoRestart;
            BuildColorMappingCaches = model.BuildColorMappingCaches;
            EnableCompressionCache = model.EnableCompressionCache;
            LoadDllAppComponents = model.LoadDllAppComponents;
            SystemAppComponents = model.SystemAppComponents.AsReadOnly();
            ServicesAppID = model.ServicesAppID;
            StartupChecklist = model.StartupChecklist.AsReadOnly();
        }

        public bool CrashAutoRestart { get; }

        public bool BuildColorMappingCaches { get; }

        public bool EnableCompressionCache { get; }

        public bool LoadDllAppComponents { get; }

        public ReadOnlyCollection<string> SystemAppComponents { get; }

        public string ServicesAppID { get; }

        public ReadOnlyCollection<string> StartupChecklist { get; }

        public static SystemConfig Load(string path)
        {
            ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));

            TomlTable table = Toml.ReadFile(path);
            Model model = table.Get<Model>();
            Model.Validate(model, Path.GetFileName(path));
            return new(model);
        }

        public static SystemConfig FromDataModel(Model model)
        {
            return new(model);
        }

        public Model ToDataModel()
        {
            return new()
            {
                CrashAutoRestart = CrashAutoRestart,
                BuildColorMappingCaches = BuildColorMappingCaches,
                LoadDllAppComponents = LoadDllAppComponents,
                SystemAppComponents = SystemAppComponents.ToArray(),
                ServicesAppID = ServicesAppID,
                StartupChecklist = StartupChecklist.ToArray()
            };
        }

        public class Model : IDataModel<Model>
        {
            public Model()
            {
                CrashAutoRestart = true;
                BuildColorMappingCaches = true;
                EnableCompressionCache = true;
                LoadDllAppComponents = false;
                SystemAppComponents = [
                    "MCBS.SystemApplications.Services",
                    "MCBS.SystemApplications.Desktop",
                    "MCBS.SystemApplications.Console",
                    "MCBS.SystemApplications.Settings",
                    "MCBS.SystemApplications.TaskManager",
                    "MCBS.SystemApplications.FileExplorer",
                    "MCBS.SystemApplications.ScreenManager",
                    "MCBS.SystemApplications.ScreenController",
                    "MCBS.SystemApplications.VideoPlayer",
                    "MCBS.SystemApplications.Album",
                    "MCBS.SystemApplications.Drawing",
                    "MCBS.SystemApplications.Notepad",
                    "MCBS.SystemApplications.QRCodeGenerator",
                    "MCBS.SystemApplications.DataScreen"
                    ];
                ServicesAppID = "System.Services";
                StartupChecklist = ["System.Desktop"];
            }

            [Display(Name = "崩溃时是否自动重启")]
            public bool CrashAutoRestart { get; set; }

            [Display(Name = "启动时是否构建Minecraft方块颜色映射表缓存")]
            public bool BuildColorMappingCaches { get; set; }

            [Display(Name = "是否启用压缩缓存，启用后大幅降低缓存内存占用，但会降低一些执行效率")]
            public bool EnableCompressionCache { get; set; }

            [Display(Name = "是否加载“MCBS\\DllAppComponents\\”目录下的DLL应用程序")]
            public bool LoadDllAppComponents { get; set; }

            [Display(Name = "系统应用程序组件加载列表")]
            [Required(ErrorMessage = ErrorMessageHelper.RequiredAttribute)]
            public string[] SystemAppComponents { get; set; }

            [Display(Name = "系统服务AppID")]
            [Required(ErrorMessage = ErrorMessageHelper.RequiredAttribute)]
            public string ServicesAppID { get; set; }

            [Display(Name = "启动项AppID列表")]
            [Required(ErrorMessage = ErrorMessageHelper.RequiredAttribute)]
            public string[] StartupChecklist { get; set; }

            public static Model CreateDefault()
            {
                return new();
            }

            public static void Validate(Model model, string name)
            {
                ValidationHelper.Validate(model, name);
            }
        }
    }
}
