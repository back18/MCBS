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
    public class ScreenConfig : IDataModelOwner<ScreenConfig, ScreenConfig.Model>
    {
        private ScreenConfig(Model model)
        {
            NullValidator.ValidateObject(model, nameof(model));

            MaxCount = model.MaxCount;
            MinLength = model.MinLength;
            MaxLength = model.MaxLength;
            MinAltitude = model.MinAltitude;
            MaxAltitude = model.MaxAltitude;
            InitialWidth = model.InitialWidth;
            InitialHeight = model.InitialHeight;
            ScreenIdleTimeout = model.ScreenIdleTimeout;
            RightClickObjective = model.RightClickObjective;
            RightClickCriterion = model.RightClickCriterion;
            RightClickItemID = model.RightClickItemID;
            TextEditorItemID = model.TextEditorItemID;
            ScreenBuilderItemName = model.ScreenBuilderItemName;
            ScreenOperatorList = model.ScreenOperatorList.AsReadOnly();
            ScreenBuildOperatorList = model.ScreenBuildOperatorList.AsReadOnly();
        }

        public int MaxCount { get; }

        public int MinLength { get; }

        public int MaxLength { get; }

        public int MinAltitude { get; }

        public int MaxAltitude { get; }

        public int InitialWidth { get; }

        public int InitialHeight { get; }

        public int ScreenIdleTimeout { get; }

        public string RightClickObjective { get; }

        public string RightClickCriterion { get; }

        public string RightClickItemID { get; }

        public string TextEditorItemID { get; }

        public string ScreenBuilderItemName { get; }

        public ReadOnlyCollection<string> ScreenOperatorList { get; }

        public ReadOnlyCollection<string> ScreenBuildOperatorList { get; }

        public static ScreenConfig Load(string path)
        {
            ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));

            TomlTable table = Toml.ReadFile(path);
            Model model = table.Get<Model>();
            Model.Validate(model, Path.GetFileName(path));
            return new(model);
        }

        public static ScreenConfig FromDataModel(Model model)
        {
            return new(model);
        }

        public Model ToDataModel()
        {
            return new()
            {
                MaxCount = MaxCount,
                MinLength = MinLength,
                MaxLength = MaxLength,
                MinAltitude = MinAltitude,
                MaxAltitude = MaxAltitude,
                InitialWidth = InitialWidth,
                InitialHeight = InitialHeight,
                RightClickObjective = RightClickObjective,
                RightClickCriterion = RightClickCriterion,
                RightClickItemID = RightClickItemID,
                TextEditorItemID = TextEditorItemID,
                ScreenBuilderItemName = ScreenBuilderItemName,
                ScreenOperatorList = ScreenOperatorList.ToArray(),
                ScreenBuildOperatorList = ScreenBuildOperatorList.ToArray()
            };
        }

        public class Model : IDataModel<Model>
        {
            public Model()
            {
                MaxCount = 8;
                MinLength = 32;
                MaxLength = 512;
                MinAltitude = -64;
                MaxAltitude = 319;
                InitialWidth = 128;
                InitialHeight = 72;
                ScreenIdleTimeout = -1;
                RightClickObjective = "snowball_mouse";
                RightClickCriterion = "minecraft.used:minecraft.snowball";
                RightClickItemID = "minecraft:snowball";
                TextEditorItemID = "minecraft:writable_book";
                ScreenBuilderItemName = "创建屏幕";
                ScreenOperatorList = [];
                ScreenBuildOperatorList = [];
            }

            [Display(Name = "屏幕最大数量")]
            [Range(0, 64, ErrorMessage = ErrorMessageHelper.Range)]
            public int MaxCount { get; set; }

            [Display(Name = "屏幕最小长度")]
            [Range(1, 512, ErrorMessage = ErrorMessageHelper.Range)]
            public int MinLength { get; set; }

            [Display(Name = "屏幕最大长度")]
            [Range(1, 512, ErrorMessage = ErrorMessageHelper.Range)]
            public int MaxLength { get; set; }

            [Display(Name = "屏幕的位置在主世界中的最小高度")]
            [Range(-2048, 2048, ErrorMessage = ErrorMessageHelper.Range)]
            public int MinAltitude { get; set; }

            [Display(Name = "屏幕的位置在主世界中的最大高度")]
            [Range(-2048, 2048, ErrorMessage = ErrorMessageHelper.Range)]
            public int MaxAltitude { get; set; }

            [Display(Name = "屏幕初始宽度")]
            [Range(1, 512, ErrorMessage = ErrorMessageHelper.Range)]
            public int InitialWidth { get; set; }

            [Display(Name = "屏幕初始高度")]
            [Range(1, 512, ErrorMessage = ErrorMessageHelper.Range)]
            public int InitialHeight { get; set; }

            [Display(Name = "屏幕闲置超时", Description = "屏幕在一段时间无操作后会自动关闭，设置为-1将无限等待，单位为Tick(50ms)")]
            [Range(-1, int.MaxValue, ErrorMessage = ErrorMessageHelper.Range)]
            public int ScreenIdleTimeout { get; set; }

            [Display(Name = "触发右键点击操作的计分板名称")]
            [Required(ErrorMessage = ErrorMessageHelper.Required)]
            public string RightClickObjective { get; set; }

            [Display(Name = "触发右键点击操作的计分板准则")]
            [Required(ErrorMessage = ErrorMessageHelper.Required)]
            public string RightClickCriterion { get; set; }

            [Display(Name = "触发右键点击操作的物品ID")]
            [Required(ErrorMessage = ErrorMessageHelper.Required)]
            public string RightClickItemID { get; set; }

            [Display(Name = "编辑屏幕文本的书与笔物品ID")]
            [Required(ErrorMessage = ErrorMessageHelper.Required)]
            public string TextEditorItemID { get; set; }

            [Display(Name = "载入屏幕构建器的物品名称")]
            [Required(ErrorMessage = ErrorMessageHelper.Required)]
            public string ScreenBuilderItemName { get; set; }

            [Display(Name = "允许控制屏幕的玩家白名单")]
            [Required(ErrorMessage = ErrorMessageHelper.Required)]
            public string[] ScreenOperatorList { get; set; }

            [Display(Name = "允许创建屏幕的玩家白名单")]
            [Required(ErrorMessage = ErrorMessageHelper.Required)]
            public string[] ScreenBuildOperatorList { get; set; }

            public static Model CreateDefault()
            {
                return new();
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
                        message.AppendLine(result.ErrorMessage);
                        count++;
                    }
                }

                if (model.MinLength > model.MaxLength)
                {
                    message.AppendLine(ErrorMessageHelper.Format($"{nameof(MinLength)}不能大于{nameof(MaxLength)}"));
                    count++;
                }

                if (model.MinAltitude > model.MaxAltitude)
                {
                    message.AppendLine(ErrorMessageHelper.Format($"{nameof(MinAltitude)}不能大于{nameof(MaxAltitude)}"));
                    count++;
                }

                if (count > 0)
                {
                    message.Insert(0, $"解析“{name}”时遇到了{count}个错误：");
                    throw new ValidationException(message.ToString().TrimEnd());
                }
            }
        }
    }
}
