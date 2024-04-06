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

namespace MCBS.Config.Minecraft
{
    public class MinecraftConfig : IDataModelOwner<MinecraftConfig, MinecraftConfig.Model>
    {
        private MinecraftConfig(Model model)
        {
            NullValidator.ValidateObject(model, nameof(model));

            MinecraftPath = model.MinecraftPath;
            MinecraftVersion = model.MinecraftVersion;
            MinecraftType = model.MinecraftType;
            ServerAddress = model.ServerAddress;
            ServerPort = (ushort)model.ServerPort;
            Language = model.Language;
            ResourcePackList = model.ResourcePackList.AsReadOnly();
            DownloadSource = model.DownloadSource;
            CommunicationMode = model.CommunicationMode;
            McapiModeConfig = McapiModeConfig.FromDataModel(model.McapiModeConfig);
            RconModeConfig = RconModeConfig.FromDataModel(model.RconModeConfig);
            ConsoleModeConfig = ConsoleModeConfig.FromDataModel(model.ConsoleModeConfig);
        }

        public string MinecraftPath { get; }

        public string MinecraftVersion { get; }

        public string MinecraftType { get; }

        public string ServerAddress { get; }

        public ushort ServerPort { get; }

        public string Language { get; }

        public ReadOnlyCollection<string> ResourcePackList { get; }

        public string DownloadSource { get; }

        public string CommunicationMode { get; }

        public McapiModeConfig McapiModeConfig { get; }

        public RconModeConfig RconModeConfig { get; }

        public ConsoleModeConfig ConsoleModeConfig { get; }

        public static MinecraftConfig FromDataModel(Model model)
        {
            return new(model);
        }

        public Model ToDataModel()
        {
            return new()
            {
                MinecraftPath = MinecraftPath,
                MinecraftVersion = MinecraftVersion,
                MinecraftType = MinecraftType,
                ServerAddress = ServerAddress,
                ServerPort = ServerPort,
                Language = Language,
                ResourcePackList = ResourcePackList.ToArray(),
                DownloadSource = DownloadSource,
                CommunicationMode = CommunicationMode
            };
        }

        public static MinecraftConfig Load(string path)
        {
            ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));

            TomlTable table = Toml.ReadFile(path);
            Model model = table.Get<Model>();
            string fileName = Path.GetFileName(path);
            Model.Validate(model, fileName);

            switch (model.CommunicationMode)
            {
                case CommunicationModes.MCAPI:
                    McapiModeConfig.Model.Validate(model.McapiModeConfig, $"{fileName}[{nameof(model.McapiModeConfig)}]");
                    break;
                case CommunicationModes.RCON:
                    RconModeConfig.Model.Validate(model.RconModeConfig, $"{fileName}[{nameof(model.RconModeConfig)}]");
                    break;
                case CommunicationModes.CONSOLE:
                    ConsoleModeConfig.Model.Validate(model.ConsoleModeConfig, $"{fileName}[{nameof(model.ConsoleModeConfig)}]");
                    break;
                case CommunicationModes.HYBRID:
                    RconModeConfig.Model.Validate(model.RconModeConfig, $"{fileName}[{nameof(model.RconModeConfig)}]");
                    ConsoleModeConfig.Model.Validate(model.ConsoleModeConfig, $"{fileName}[{nameof(model.ConsoleModeConfig)}]");
                    break;
                default:
                    throw new InvalidOperationException();
            }

            return new(model);
        }

        public class Model : IDataModel<Model>
        {
            public Model()
            {
                MinecraftPath = "";
                MinecraftVersion = "1.20.1";
                MinecraftType = MinecraftTypes.CLIENT;
                ServerAddress = "localhost";
                ServerPort = 25565;
                Language = "zh_cn";
                ResourcePackList = [];
                DownloadSource = DownloadSources.BMCLAPI;
                CommunicationMode = CommunicationModes.MCAPI;
                McapiModeConfig = Minecraft.McapiModeConfig.Model.CreateDefault();
                RconModeConfig = Minecraft.RconModeConfig.Model.CreateDefault();
                ConsoleModeConfig = Minecraft.ConsoleModeConfig.Model.CreateDefault();
            }

            [Display(Name = "Minecraft路径", Description = "用于确定Minecra主目录所在路径\n\".\"为程序工作目录\n\"..\"为程序工作目录的上一层目录")]
            [Required(ErrorMessage = ErrorMessageHelper.RequiredAttribute)]
            [DirectoryExists]
            public string MinecraftPath { get; set; }

            [Display(Name = "Minecraft版本", Description = "用于确定Minecraft的游戏版本")]
            [Required(ErrorMessage = ErrorMessageHelper.RequiredAttribute)]
            public string MinecraftVersion { get; set; }

            [Display(Name = "Minecraft类型", Description = "用于确定Minecraft是服务端还是客户端")]
            [Required(ErrorMessage = ErrorMessageHelper.RequiredAttribute)]
            [NewAllowedValues(MinecraftTypes.CLIENT, MinecraftTypes.SERVER, ErrorMessage = ErrorMessageHelper.NewAllowedValuesAttribute)]
            public string MinecraftType { get; set; }

            [Display(Name = "Minecraft服务器地址", Description = "也作为RCON地址以及MCAPI地址\n由于一般情况下MCBS与Minecraft均运行在同一台主机\n因此将其设置为localhost即可")]
            [RequiredIf(nameof(MinecraftType), CompareOperator.Equal, MinecraftTypes.SERVER, ErrorMessage = ErrorMessageHelper.RequiredIfAttribute)]
            public string ServerAddress { get; set; }

            [Display(Name = "Minecraft服务器端口")]
            [RequiredIf(nameof(MinecraftType), CompareOperator.Equal, MinecraftTypes.SERVER, ErrorMessage = ErrorMessageHelper.RequiredIfAttribute)]
            [Range(ushort.MinValue, ushort.MaxValue, ErrorMessage = ErrorMessageHelper.RangeAttribute)]
            public int ServerPort { get; set; }

            [Display(Name = "语言标识", Description = "服务端语言默认为en_us，客户端根据选择的语言设置\n主要影响命令结果文本的解析\n语言文件目录: MCBS\\Minecraft\\Vanilla\\{版本}\\Languages\\")]
            [Required(ErrorMessage = ErrorMessageHelper.RequiredAttribute)]
            public string Language { get; set; }

            [Display(Name = "资源包列表", Description = "程序会根据资源包列表按照顺序加载资源包文件\n支持的文件类型: 客户端核心.jar, 服务端核心.jar, 模组文件.jar, 资源包.zip\n资源包目录: MCBS\\Minecraft\\ResourcePacks\\")]
            [Required(ErrorMessage = ErrorMessageHelper.RequiredAttribute)]
            public string[] ResourcePackList { get; set; }

            [Display(Name = "下载源", Description = "用于确定下载Minecraft游戏资源时使用的下载源")]
            [Required(ErrorMessage = ErrorMessageHelper.RequiredAttribute)]
            [NewAllowedValues(DownloadSources.MOJANG, DownloadSources.BMCLAPI, ErrorMessage = ErrorMessageHelper.NewAllowedValuesAttribute)]
            public string DownloadSource { get; set; }

            [Display(Name = "通信模式", Description = "用于确定与Minecraft实例的通信模式\nMCAPI: 连接到已启动的Minecraft服务端，使用MCAPI模组进行通信，支持服务端和客户端\nRCON: 连接到已启动的Minecraft服务端，使用RCON进行通信，仅支持服务端\nCONSOLE: 启动一个新的Minecraft服务端进程，使用控制台输入输出流进行通信，仅支持服务端\nHYBRID: 启动一个新的Minecraft服务端进程，发送单条命令时使用RCON，发送批量命令时使用控制台输入输出流，仅支持服务端")]
            [Required(ErrorMessage = ErrorMessageHelper.RequiredAttribute)]
            [NewAllowedValues(CommunicationModes.MCAPI, CommunicationModes.RCON, CommunicationModes.CONSOLE, CommunicationModes.HYBRID, ErrorMessage = ErrorMessageHelper.NewAllowedValuesAttribute)]
            [AllowedValuesIf(nameof(MinecraftType), CompareOperator.Equal, MinecraftTypes.CLIENT, CommunicationModes.MCAPI)]
            public string CommunicationMode { get; set; }

            [Display(Name = "MCAPI模式配置")]
            [RequiredIf(nameof(CommunicationMode), CompareOperator.Equal, CommunicationModes.MCAPI, ErrorMessage = ErrorMessageHelper.RequiredIfAttribute)]
            public McapiModeConfig.Model McapiModeConfig { get; set; }

            [Display(Name = "RCON模式配置")]
            [RequiredIf(nameof(CommunicationMode), CompareOperator.Equal, CommunicationModes.RCON, ErrorMessage = ErrorMessageHelper.RequiredIfAttribute)]
            [RequiredIf(nameof(CommunicationMode), CompareOperator.Equal, CommunicationModes.HYBRID, ErrorMessage = ErrorMessageHelper.RequiredIfAttribute)]
            public RconModeConfig.Model RconModeConfig { get; set; }

            [Display(Name = "CONSOLE模式配置")]
            [RequiredIf(nameof(CommunicationMode), CompareOperator.Equal, CommunicationModes.CONSOLE, ErrorMessage = ErrorMessageHelper.RequiredIfAttribute)]
            [RequiredIf(nameof(CommunicationMode), CompareOperator.Equal, CommunicationModes.HYBRID, ErrorMessage = ErrorMessageHelper.RequiredIfAttribute)]
            public ConsoleModeConfig.Model ConsoleModeConfig { get; set; }

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
