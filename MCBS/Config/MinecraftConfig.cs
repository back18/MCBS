using MCBS.Config.Constants;
using Nett;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QuanLib.Core;
using QuanLib.DataAnnotations;
using QuanLib.Minecraft;
using QuanLib.Minecraft.ResourcePack;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Config
{
    public class MinecraftConfig : IDataModelOwner<MinecraftConfig, MinecraftConfig.Model>
    {
        private MinecraftConfig(Model model)
        {
            NullValidator.ValidateObject(model, nameof(model));

            GameVersion = model.GameVersion;
            MinecraftType = model.MinecraftType;
            CommunicationMode = model.CommunicationMode;
            DownloadSource = model.DownloadSource;
            MinecraftPath = model.MinecraftPath;
            ServerAddress = model.ServerAddress;
            JavaPath = model.JavaPath;
            LaunchArguments = model.LaunchArguments;
            McapiPort = (ushort)model.McapiPort;
            McapiPassword = model.McapiPassword;
            Language = model.Language;
            ResourcePackList = new(model.ResourcePackList);
        }

        public string GameVersion { get; }

        public string MinecraftType { get; }

        public string CommunicationMode { get; }

        public string DownloadSource { get; }

        public string MinecraftPath { get; }

        public string ServerAddress { get; }

        public string JavaPath { get; }

        public string LaunchArguments { get; }

        public ushort McapiPort { get; }

        public string McapiPassword { get; }

        public string Language { get; }

        public ReadOnlyCollection<string> ResourcePackList { get; }

        public static MinecraftConfig FromDataModel(Model model)
        {
            return new(model);
        }

        public Model ToDataModel()
        {
            return new()
            {
                GameVersion = GameVersion,
                MinecraftType = MinecraftType,
                CommunicationMode = CommunicationMode,
                DownloadSource = DownloadSource,
                MinecraftPath = MinecraftPath,
                ServerAddress = ServerAddress,
                JavaPath = JavaPath,
                LaunchArguments = LaunchArguments,
                McapiPort = McapiPort,
                McapiPassword = McapiPassword,
                Language = Language,
                ResourcePackList = ResourcePackList.ToArray(),
            };
        }

        public static MinecraftConfig Load(string path)
        {
            ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));

            TomlTable table = Toml.ReadFile(path);
            Model model = table.Get<Model>();
            Model.Validate(model, Path.GetFileName(path));
            return new(model);
        }

        public class Model : IDataModel<Model>
        {
            public Model()
            {
                GameVersion = "1.20.1";
                MinecraftType = MinecraftTypes.CLIENT;
                CommunicationMode = CommunicationModes.MCAPI;
                DownloadSource = DownloadSources.BMCLAPI;
                MinecraftPath = "";
                ServerAddress = "127.0.0.1";
                JavaPath = "";
                LaunchArguments = "";
                McapiPort = 25585;
                McapiPassword = "";
                Language = "zh_cn";
                ResourcePackList = [];
            }

            [Display(Name = "游戏版本", Description = "用于确定Minecraft的游戏版本")]
            [Required(ErrorMessage = ErrorMessageHelper.RequiredAttribute)]
            public string GameVersion { get; set; }

            [Display(Name = "Minecraft类型", Description = "用于确定Minecraft是服务端还是客户端")]
            [Required(ErrorMessage = ErrorMessageHelper.RequiredAttribute)]
            [NewAllowedValues(MinecraftTypes.SERVER, MinecraftTypes.CLIENT, ErrorMessage = ErrorMessageHelper.NewAllowedValuesAttribute)]
            public string MinecraftType { get; set; }

            [Display(Name = "通信模式", Description = "用于确定与Minecraft实例的通信模式\nRCON: 连接到已启动的Minecraft服务端，使用RCON进行通信，仅支持服务端\nCONSOLE: 启动一个新的Minecraft服务端进程，使用控制台输入输出流进行通信，仅支持服务端\nHYBRID: 启动一个新的Minecraft服务端进程，发送单条命令时使用RCON，发送批量命令时使用控制台输入输出流，仅支持服务端\nMCAPI: 连接到已启动的Minecraft服务端，使用MCAPI模组进行通信，支持服务端和客户端")]
            [Required(ErrorMessage = ErrorMessageHelper.RequiredAttribute)]
            [NewAllowedValues(CommunicationModes.RCON, CommunicationModes.CONSOLE, CommunicationModes.HYBRID, CommunicationModes.MCAPI, ErrorMessage = ErrorMessageHelper.NewAllowedValuesAttribute)]
            [AllowedValuesIf(nameof(MinecraftType), CompareOperator.Equal, "CLIENT", "MCAPI")]
            public string CommunicationMode { get; set; }

            [Display(Name = "下载源", Description = "用于确定下载Minecraft游戏资源时使用的下载源")]
            [Required(ErrorMessage = ErrorMessageHelper.RequiredAttribute)]
            [NewAllowedValues(DownloadSources.MOJANG, DownloadSources.BMCLAPI, ErrorMessage = ErrorMessageHelper.NewAllowedValuesAttribute)]
            public string DownloadSource { get; set; }

            [Display(Name = "Minecraft路径", Description = "用于确定Minecra主目录所在路径\n\".\"为程序工作目录\n\"..\"为程序工作目录的上一层目录")]
            [Required(ErrorMessage = ErrorMessageHelper.RequiredAttribute)]
            [DirectoryExists]
            public string MinecraftPath { get; set; }

            [Display(Name = "服务器地址", Description = "根据不同模式，也作为RCON地址或MCAPI地址")]
            [RequiredIf(nameof(CommunicationMode), CompareOperator.Equal, "RCON", ErrorMessage = ErrorMessageHelper.RequiredIfAttribute)]
            [RequiredIf(nameof(CommunicationMode), CompareOperator.Equal, "HYBRID", ErrorMessage = ErrorMessageHelper.RequiredIfAttribute)]
            [RequiredIf(nameof(CommunicationMode), CompareOperator.Equal, "MCAPI", ErrorMessage = ErrorMessageHelper.RequiredIfAttribute)]
            public string ServerAddress { get; set; }

            [Display(Name = "Java路径", Description = "启动Minecraft服务端进程所使用的Java路径")]
            [RequiredIf(nameof(CommunicationMode), CompareOperator.Equal, "CONSOLE", ErrorMessage = ErrorMessageHelper.RequiredIfAttribute)]
            [RequiredIf(nameof(CommunicationMode), CompareOperator.Equal, "HYBRID", ErrorMessage = ErrorMessageHelper.RequiredIfAttribute)]
            public string JavaPath { get; set; }

            [Display(Name = "启动参数", Description = "启动Minecraft服务端进程所使用的启动参数")]
            [RequiredIf(nameof(CommunicationMode), CompareOperator.Equal, "CONSOLE", ErrorMessage = ErrorMessageHelper.RequiredIfAttribute)]
            [RequiredIf(nameof(CommunicationMode), CompareOperator.Equal, "HYBRID", ErrorMessage = ErrorMessageHelper.RequiredIfAttribute)]
            public string LaunchArguments { get; set; }

            [Display(Name = "MCAPI端口")]
            [Range(0, 65535, ErrorMessage = ErrorMessageHelper.RangeAttribute)]
            public int McapiPort { get; set; }

            [Display(Name = "MCAPI登录密码")]
            [RequiredIf(nameof(CommunicationMode), CompareOperator.Equal, "MCAPI", ErrorMessage = ErrorMessageHelper.RequiredIfAttribute)]
            public string McapiPassword { get; set; }

            [Display(Name = "语言标识", Description = "服务端语言默认为en_us，客户端根据选择的语言设置\n主要影响命令结果文本的解析\n语言文件目录: MCBS\\Minecraft\\Vanilla\\{版本}\\Languages\\")]
            [Required(ErrorMessage = ErrorMessageHelper.RequiredAttribute)]
            public string Language { get; set; }

            [Display(Name = "资源包列表", Description = "程序会根据资源包列表按照顺序加载资源包文件\n支持的文件类型: 客户端核心.jar, 服务端核心.jar, 模组文件.jar, 资源包.zip\n资源包目录: MCBS\\Minecraft\\ResourcePacks\\")]
            [Required(ErrorMessage = ErrorMessageHelper.RequiredAttribute)]
            public string[] ResourcePackList { get; set; }

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
