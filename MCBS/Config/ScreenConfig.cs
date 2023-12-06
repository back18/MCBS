using MCBS.Screens;
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
    public class ScreenConfig
    {
        private ScreenConfig(Model model)
        {
            NullValidator.ValidateObject(model, nameof(model));

            MaxCount = model.MaxCount;
            MinLength = model.MinLength;
            MaxLength = model.MaxLength;
            MinPixels = model.MinPixels;
            MaxPixels = model.MaxPixels;
            MinY = model.MinY;
            MaxY = model.MaxY;
            ScreenBuildTimeout = model.ScreenBuildTimeout;
            ScreenIdleTimeout = model.ScreenIdleTimeout;
            RightClickObjective = model.RightClickObjective;
            RightClickCriterion = model.RightClickCriterion;
            RightClickItemID = model.RightClickItemID;
            TextEditorItemID = model.TextEditorItemID;
            ScreenBuilderItemName = model.ScreenBuilderItemName;
            ScreenOperatorList = new(model.ScreenOperatorList);
            ScreenBuildOperatorList = new(model.ScreenBuildOperatorList);

            List<ScreenOptions> list = new();
            foreach (var item in model.ResidentScreenList)
            {
                list.Add(new(item));
            }
            ResidentScreenList = list.AsReadOnly();
        }

        public int MaxCount { get; }

        public int MinLength { get; }

        public int MaxLength { get; }

        public int MinPixels { get; }

        public int MaxPixels { get; }

        public int MinY { get; }

        public int MaxY { get; }

        public int ScreenBuildTimeout { get; }

        public int ScreenIdleTimeout { get; }

        public string RightClickObjective { get; }

        public string RightClickCriterion { get; }

        public string RightClickItemID { get; }

        public string TextEditorItemID { get; }

        public string ScreenBuilderItemName { get; }

        public ReadOnlyCollection<string> ScreenOperatorList { get; }

        public ReadOnlyCollection<string> ScreenBuildOperatorList { get; }

        public ReadOnlyCollection<ScreenOptions> ResidentScreenList { get; }

        public static ScreenConfig Load(string path)
        {
            ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));

            TomlTable table = Toml.ReadFile(path);
            Model model = table.Get<Model>();
            Validate(model, Path.GetFileName(path));
            return new(model);
        }

        public static void Validate(Model model, string name)
        {
            ArgumentNullException.ThrowIfNull(model, nameof(model));
            ArgumentException.ThrowIfNullOrEmpty(name, nameof(name));

            List<ValidationResult> results = new();
            StringBuilder message = new();
            message.AppendLine();
            int count = 0;

            if (!Validator.TryValidateObject(model, new(model), results, true))
            {
                foreach (var result in results)
                {
                    string memberName = result.MemberNames.FirstOrDefault() ?? string.Empty;
                    message.AppendLine($"[{memberName}]: {result.ErrorMessage}");
                    count++;
                }
            }

            if (model.MinLength > model.MaxLength)
            {
                message.AppendLine($"[MinLength]: MinLength 不能大于 MaxLength");
                count++;
            }

            if (model.MinPixels > model.MaxPixels)
            {
                message.AppendLine($"[MinPixels]: MinPixels 不能大于 MaxPixels");
                count++;
            }

            if (model.MinY > model.MaxY)
            {
                message.AppendLine($"[MinY]: MinY 不能大于 MaxY");
                count++;
            }

            if (count > 0)
            {
                message.Insert(0, $"解析“{name}”时遇到{count}个错误：");
                throw new ValidationException(message.ToString().TrimEnd());
            }

            for (int i = 0; i < model.ResidentScreenList.Length; i++)
            {
                ScreenOptions.Validate($"{name}->ResidentScreenList[{i}]", model.ResidentScreenList[i]);
            }
        }

        public class Model
        {
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。

            [Range(0, 64, ErrorMessage = "值的范围应该为0~64")]
            public int MaxCount { get; set; }

            [Range(1, 512, ErrorMessage = "值的范围应该为1~512")]
            public int MinLength { get; set; }

            [Range(1, 512, ErrorMessage = "值的范围应该为1~512")]
            public int MaxLength { get; set; }

            [Range(1, 262144, ErrorMessage = "值的范围应该为1~262144")]
            public int MinPixels { get; set; }

            [Range(1, 262144, ErrorMessage = "值的范围应该为1~262144")]
            public int MaxPixels { get; set; }

            public int MinY { get; set; }

            public int MaxY { get; set; }

            [Range(-1, int.MaxValue, ErrorMessage = "值的范围应该为-1~214748367")]
            public int ScreenBuildTimeout { get; set; }

            [Range(-1, int.MaxValue, ErrorMessage = "值的范围应该为-1~214748367")]
            public int ScreenIdleTimeout { get; set; }

            [Required(ErrorMessage = "配置项缺失")]
            public string RightClickObjective { get; set; }

            [Required(ErrorMessage = "配置项缺失")]
            public string RightClickCriterion { get; set; }

            [Required(ErrorMessage = "配置项缺失")]
            public string RightClickItemID { get; set; }

            [Required(ErrorMessage = "配置项缺失")]
            public string TextEditorItemID { get; set; }

            [Required(ErrorMessage = "配置项缺失")]
            public string ScreenBuilderItemName { get; set; }

            [Required(ErrorMessage = "配置项缺失")]
            public string[] ScreenOperatorList { get; set; }

            [Required(ErrorMessage = "配置项缺失")]
            public string[] ScreenBuildOperatorList { get; set; }

            [Required(ErrorMessage = "配置项缺失")]
            public ScreenOptions.Model[] ResidentScreenList { get; set; }

#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        }
    }
}
