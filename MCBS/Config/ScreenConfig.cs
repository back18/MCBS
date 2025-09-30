﻿using Nett;
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

            List<BlockState> list = new();
            foreach (var item in model.ScreenBlockBlacklist)
            {
                if (BlockState.TryParse(item, out var blockState))
                    list.Add(blockState);
            }
            ScreenBlockBlacklist = list.AsReadOnly();
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

        public ReadOnlyCollection<BlockState> ScreenBlockBlacklist { get; }

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
                ScreenBuildOperatorList = ScreenBuildOperatorList.ToArray(),
                ScreenBlockBlacklist = ScreenBlockBlacklist.Select(x => x.ToString()).ToArray()
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
                InitialWidth = 128 + 32;
                InitialHeight = 72 + 32;
                ScreenIdleTimeout = -1;
                RightClickObjective = "snowball_mouse";
                RightClickCriterion = "minecraft.used:minecraft.snowball";
                RightClickItemID = "minecraft:snowball";
                TextEditorItemID = "minecraft:writable_book";
                ScreenBuilderItemName = "创建屏幕";
                ScreenOperatorList = [];
                ScreenBuildOperatorList = [];
                ScreenBlockBlacklist = [
                    "minecraft:glowstone",                  //荧石
                    "minecraft:jack_o_lantern",             //南瓜灯
                    "minecraft:sea_lantern",                //海晶灯
                    "minecraft:ochre_froglight",            //赭黄蛙明灯
                    "minecraft:verdant_froglight",          //青翠蛙明灯
                    "minecraft:pearlescent_froglight",      //珠光蛙明灯
                    "minecraft:shroomlight",                //菌光体
                    "minecraft:redstone_lamp[lit=true]",    //红石灯（点亮）
                    "minecraft:crying_obsidian",            //哭泣的黑曜石
                    "minecraft:magma_block",                //岩浆块
                    "minecraft:sculk_catalyst",             //幽匿催发体
                    "minecraft:beacon",                     //信标
                    "minecraft:respawn_anchor[charges=1]",  //重生锚（充能等级1）
                    "minecraft:respawn_anchor[charges=2]",  //重生锚（充能等级2）
                    "minecraft:respawn_anchor[charges=3]",  //重生锚（充能等级2）
                    "minecraft:respawn_anchor[charges=4]",  //重生锚（充能等级4）
                    "minecraft:furnace[lit=true]",          //熔炉（燃烧中）
                    "minecraft:smoker[lit=true]",           //烟熏炉（燃烧中）
                    "minecraft:blast_furnace[lit=true]",    //高炉（燃烧中）
                    "minecraft:redstone_ore[lit=true]",     //红石矿石（激活）
                    "minecraft:deepslate_redstone_ore[lit=true]",   //深层红石矿石（激活）
                    "minecraft:grass_block",                //草方块
                    "minecraft:podzol",                     //灰化土
                    "minecraft:mycelium",                   //菌丝体
                    "minecraft:crimson_nylium",             //绯红菌岩
                    "minecraft:warped_nylium",              //诡异菌岩
                    "minecraft:carved_pumpkin",             //雕刻南瓜
                    "minecraft:tnt",                        //TNT
                    "minecraft:snow",                       //雪
                    "minecraft:ice",                        //冰
                    "minecraft:budding_amethyst",           //紫水晶母岩
                    "minecraft:tube_coral_block",           //管珊瑚块
                    "minecraft:brain_coral_block",          //脑纹珊瑚块
                    "minecraft:bubble_coral_block",         //气泡珊瑚块
                    "minecraft:fire_coral_block",           //火珊瑚块
                    "minecraft:horn_coral_block",           //鹿角珊瑚块
                ];
            }

            [Display(Name = "屏幕最大数量")]
            [Range(0, 64, ErrorMessage = ErrorMessageHelper.RangeAttribute)]
            public int MaxCount { get; set; }

            [Display(Name = "屏幕最小长度")]
            [Range(1, 512, ErrorMessage = ErrorMessageHelper.RangeAttribute)]
            public int MinLength { get; set; }

            [Display(Name = "屏幕最大长度")]
            [Range(1, 512, ErrorMessage = ErrorMessageHelper.RangeAttribute)]
            [GreaterThan(nameof(MinLength), ErrorMessage = ErrorMessageHelper.GreaterThanAttribute)]
            public int MaxLength { get; set; }

            [Display(Name = "屏幕的位置在主世界中的最小高度")]
            [Range(-2048, 2048, ErrorMessage = ErrorMessageHelper.RangeAttribute)]
            public int MinAltitude { get; set; }

            [Display(Name = "屏幕的位置在主世界中的最大高度")]
            [Range(-2048, 2048, ErrorMessage = ErrorMessageHelper.RangeAttribute)]
            [GreaterThan(nameof(MinAltitude), ErrorMessage = ErrorMessageHelper.GreaterThanAttribute)]
            public int MaxAltitude { get; set; }

            [Display(Name = "屏幕初始宽度")]
            [Range(1, 512, ErrorMessage = ErrorMessageHelper.RangeAttribute)]
            public int InitialWidth { get; set; }

            [Display(Name = "屏幕初始高度")]
            [Range(1, 512, ErrorMessage = ErrorMessageHelper.RangeAttribute)]
            public int InitialHeight { get; set; }

            [Display(Name = "屏幕闲置超时", Description = "屏幕在一段时间无操作后会自动关闭，设置为-1将无限等待，单位为Tick(50ms)")]
            [Range(-1, int.MaxValue, ErrorMessage = ErrorMessageHelper.RangeAttribute)]
            public int ScreenIdleTimeout { get; set; }

            [Display(Name = "触发右键点击操作的计分板名称")]
            [Required(ErrorMessage = ErrorMessageHelper.RequiredAttribute)]
            public string RightClickObjective { get; set; }

            [Display(Name = "触发右键点击操作的计分板准则")]
            [Required(ErrorMessage = ErrorMessageHelper.RequiredAttribute)]
            public string RightClickCriterion { get; set; }

            [Display(Name = "触发右键点击操作的物品ID")]
            [Required(ErrorMessage = ErrorMessageHelper.RequiredAttribute)]
            public string RightClickItemID { get; set; }

            [Display(Name = "编辑屏幕文本的书与笔物品ID")]
            [Required(ErrorMessage = ErrorMessageHelper.RequiredAttribute)]
            public string TextEditorItemID { get; set; }

            [Display(Name = "载入屏幕构建器的物品名称")]
            [Required(ErrorMessage = ErrorMessageHelper.RequiredAttribute)]
            public string ScreenBuilderItemName { get; set; }

            [Display(Name = "允许控制屏幕的玩家白名单")]
            [Required(ErrorMessage = ErrorMessageHelper.RequiredAttribute)]
            public string[] ScreenOperatorList { get; set; }

            [Display(Name = "允许创建屏幕的玩家白名单")]
            [Required(ErrorMessage = ErrorMessageHelper.RequiredAttribute)]
            public string[] ScreenBuildOperatorList { get; set; }

            [Display(Name = "屏幕方块黑名单", Description = "将方块ID添加到黑名单后，屏幕将不再把像素映射至此方块")]
            [Required(ErrorMessage = ErrorMessageHelper.RequiredAttribute)]
            public string[] ScreenBlockBlacklist { get; set; }

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
