using Nett;
using Newtonsoft.Json;
using QuanLib.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
            NullValidator.ValidateObject(model, nameof(model));

            CrashAutoRestart = model.CrashAutoRestart;
            LoadDllAppComponents = model.LoadDllAppComponents;
            ServicesAppID = model.ServicesAppID;
            StartupChecklist = new(model.StartupChecklist);
        }

        public bool CrashAutoRestart { get; }

        public bool LoadDllAppComponents { get; }

        public string ServicesAppID { get; }

        public ReadOnlyCollection<string> StartupChecklist { get; }

        public static SystemConfig Load(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException($"“{nameof(path)}”不能为 null 或空。", nameof(path));

            TomlTable table = Toml.ReadFile(path);
            Model model = table.Get<Model>();
            Validate(model, Path.GetFileName(path));
            return new(model);
        }

        public static void Validate(Model model, string name)
        {
            if (model is null)
                throw new ArgumentNullException(nameof(model));
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException($"“{nameof(name)}”不能为 null 或空。", nameof(name));

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

            public bool LoadDllAppComponents { get; set; }

            [Required(ErrorMessage = "系统服务AppID不能为空")]
            public string ServicesAppID { get; set; }

            [Required(ErrorMessage = "配置项缺失")]
            public string[] StartupChecklist { get; set; }

#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        }
    }
}
