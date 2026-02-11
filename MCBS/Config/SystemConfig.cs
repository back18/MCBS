using Newtonsoft.Json;
using QuanLib.Core;
using QuanLib.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Config
{
    public class SystemConfig : IDataViewModel<SystemConfig>
    {
        public SystemConfig(Model model)
        {
            NullValidator.ValidateObject(model, nameof(model));

            AutoRestart = model.AutoRestart;
            BuildColorMappingCaches = model.BuildColorMappingCaches;
            EnableCompressionCache = model.EnableCompressionCache;
            LoadDllAppComponents = model.LoadDllAppComponents;
            SystemAppComponents = model.SystemAppComponents.AsReadOnly();
            ServicesAppId = model.ServicesAppId;
            StartupChecklist = model.StartupChecklist.AsReadOnly();
        }

        public bool AutoRestart { get; }

        public bool BuildColorMappingCaches { get; }

        public bool EnableCompressionCache { get; }

        public bool LoadDllAppComponents { get; }

        public ReadOnlyCollection<string> SystemAppComponents { get; }

        public string ServicesAppId { get; }

        public ReadOnlyCollection<string> StartupChecklist { get; }

        public static SystemConfig FromDataModel(object model)
        {
            return new SystemConfig((Model)model);
        }

        public object ToDataModel()
        {
            return new Model()
            {
                AutoRestart = AutoRestart,
                BuildColorMappingCaches = BuildColorMappingCaches,
                LoadDllAppComponents = LoadDllAppComponents,
                SystemAppComponents = SystemAppComponents.ToArray(),
                ServicesAppId = ServicesAppId,
                StartupChecklist = StartupChecklist.ToArray()
            };
        }

        public class Model : IDataModel<Model>
        {
            public Model()
            {
                AutoRestart = false;
                BuildColorMappingCaches = true;
                EnableCompressionCache = false;
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
                    "MCBS.SystemApplications.DataScreen",
                    "MCBS.SystemApplications.FileCreateHandler",
                    "MCBS.SystemApplications.FileDeleteHandler",
                    "MCBS.SystemApplications.FileCopyHandler",
                    "MCBS.SystemApplications.FileMoveHandler",
                    "MCBS.SystemApplications.FileRenameHandler"
                    ];
                ServicesAppId = "System.Services";
                StartupChecklist = ["System.Desktop"];
            }

            [Display(Order = 0, Name = "自动重启", Description = "启用后，将在系统发生错误时自动重启系统")]
            public bool AutoRestart { get; set; }

            [Display(Order = 1, Name = "缓存构建", Description = "启用后，将在启动时自动构建Minecraft方块颜色映射表缓存")]
            public bool BuildColorMappingCaches { get; set; }

            [Display(Order = 2, Name = "缓存压缩", Description = "启用后，将使用压缩后的缓存，大幅降低缓存内存占用，但会造成一些执行性能损失")]
            public bool EnableCompressionCache { get; set; }

            [Display(Order = 3, Name = "加载外部应用", Description = "启用后，将加载“MCBS\\DllAppComponents\\”目录下的DLL应用程序组件，请确保已完全信任DLL文件的提供者")]
            public bool LoadDllAppComponents { get; set; }

            [Display(Order = 4, Name = "系统应用列表")]
            [Required(ErrorMessage = ErrorMessageHelper.RequiredAttribute)]
            public string[] SystemAppComponents { get; set; }

            [Display(Order = 5, Name = "系统服务应用")]
            [Required(ErrorMessage = ErrorMessageHelper.RequiredAttribute)]
            public string ServicesAppId { get; set; }

            [Display(Order = 6, Name = "启动项应用列表")]
            [Required(ErrorMessage = ErrorMessageHelper.RequiredAttribute)]
            public string[] StartupChecklist { get; set; }

            public static Model CreateDefault()
            {
                return new Model();
            }

            public IValidatableObject GetValidator()
            {
                return new ValidatableObject(this);
            }

            public IEnumerable<IValidatable> GetValidatableProperties()
            {
                return Array.Empty<IValidatable>();
            }
        }
    }
}
