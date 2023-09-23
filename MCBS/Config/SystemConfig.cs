using Nett;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Config
{
    public class SystemConfig
    {
        public SystemConfig(Model model)
        {
            if (model is null)
                throw new ArgumentNullException(nameof(model));
            ;
            this.CrashAutoRestart = model.CrashAutoRestart;
            this.ServicesAppID = model.ServicesAppID;
            this.StartupChecklist = model.StartupChecklist;
            this.EnableExternalApps = model.EnableExternalApps;
            this.ExternalAppsFolder = model.ExternalAppsFolder;
        }

        public bool CrashAutoRestart { get; }

        public string ServicesAppID { get; }

        public IReadOnlyList<string> StartupChecklist { get; }

        public bool EnableExternalApps { get; }

        public string ExternalAppsFolder { get; }

        public static SystemConfig Load(string path)
        {
            TomlTable table = Toml.ReadFile(path);
            Model model = table.Get<Model>();
            Validate(Path.GetFileName(path), model);
            return new(model);
        }

        public static void Validate(string name, Model model)
        {
            if (model is null)
                throw new ArgumentNullException(nameof(model));

            List<ValidationResult> results = new();
            if (!Validator.TryValidateObject(model, new(model), results, true))
            {
                StringBuilder message = new();
                message.AppendLine();
                int count = 0;
                foreach (var result in results)
                {
                    string memberName = result.MemberNames.FirstOrDefault() ?? string.Empty;
                    message.AppendLine($"[{memberName}]: {result.ErrorMessage}");
                    count++;
                }

                if (count > 0)
                {
                    message.Insert(0, $"解析“{name}”时遇到{count}个错误：");
                    throw new ValidationException(message.ToString().TrimEnd());
                }
            }
        }

        public class Model
        {
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。

            public bool CrashAutoRestart { get; set; }

            [Required(ErrorMessage = "系统服务AppID不能为空")]
            public string ServicesAppID { get; set; }

            [Required(ErrorMessage = "配置项缺失")]
            public string[] StartupChecklist { get; set; }

            public bool EnableExternalApps { get; set; }

            [Required(ErrorMessage = "必须指定外部程序加载位置（即使未开启外部程序加载）")]
            public string ExternalAppsFolder { get; set; }


#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        }
    }
}
