using Nett;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QuanLib.Core;
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
    public class MinecraftConfig
    {
        private MinecraftConfig(Model model)
        {
            NullValidator.ValidateObject(model, nameof(model));

            DownloadApi = model.DownloadApi;
            GameVersion = model.GameVersion;
            InstanceType = model.InstanceType;
            CommunicationMode = model.CommunicationMode;
            MinecraftPath = model.MinecraftPath;
            ServerAddress = model.ServerAddress;
            JavaPath = model.JavaPath;
            LaunchArguments = model.LaunchArguments;
            McapiPort = (ushort)model.McapiPort;
            McapiPassword = model.McapiPassword;
            Language = model.Language;
            ResourcePackList = new(model.ResourcePackList);

            List<BlockState> list = new();
            foreach (var item in model.BlockTextureBlacklist)
            {
                if (BlockState.TryParse(item, out var blockState))
                    list.Add(blockState);
            }
            BlockTextureBlacklist = list.AsReadOnly();
        }

        public string DownloadApi { get; }

        public string GameVersion { get; }

        public string InstanceType { get; }

        public string CommunicationMode { get; }

        public string MinecraftPath { get; }

        public string ServerAddress { get; }

        public string JavaPath { get; }

        public string LaunchArguments { get; }

        public ushort McapiPort { get; }

        public string McapiPassword { get; }

        public string Language { get; }

        public ReadOnlyCollection<string> ResourcePackList { get; }

        public ReadOnlyCollection<BlockState> BlockTextureBlacklist { get; }

        public static MinecraftConfig Load(string path)
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

            bool isConsole = model.CommunicationMode is CommunicationModes.CONSOLE or CommunicationModes.HYBRID;
            bool isMcapi = model.CommunicationMode is CommunicationModes.MCAPI;

            if (!Validator.TryValidateObject(model, new(model), results, true))
            {
                foreach (var result in results)
                {
                    string memberName = result.MemberNames.FirstOrDefault() ?? string.Empty;
                    switch (memberName)
                    {
                        case "JavaPath":
                            if (!isConsole)
                            {
                                model.JavaPath = string.Empty;
                                continue;
                            }
                            break;
                        case "LaunchArguments":
                            if (!isConsole)
                            {
                                model.LaunchArguments = string.Empty;
                                continue;
                            }
                            break;
                        case "McapiPassword":
                            if (!isMcapi)
                            {
                                model.McapiPassword = string.Empty;
                                continue;
                            }
                            break;
                    }
                    message.AppendLine($"[{memberName}]: {result.ErrorMessage}");
                    count++;
                }
            }

            if (!Directory.Exists(model.MinecraftPath))
            {
                message.AppendLine($"[MinecraftPath]: 目录不存在: {model.MinecraftPath}");
                count++;
            }

            if (model.InstanceType == InstanceTypes.CLIENT && model.CommunicationMode != CommunicationModes.MCAPI)
            {
                message.AppendLine("[CommunicationMode]: 当配置项 InstanceType 的值为 CLIENT 时，当前配置项的值只能为 MCAPI");
                count++;
            }

            foreach (string resourcePack in model.ResourcePackList)
            {
                if (!SR.McbsDirectory.MinecraftDir.ResourcePacksDir.ExistsFile(resourcePack))
                {
                    message.AppendLine($"[ResourcePackList]:  目录不存在: {resourcePack}");
                    count++;
                }
            }

            if (count > 0)
            {
                message.Insert(0, $"解析“{name}”时遇到{count}个错误：");
                throw new ValidationException(message.ToString().TrimEnd());
            }
        }

        public class Model
        {
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。

            [Required(ErrorMessage = "配置项缺失")]
            [AllowedValues("MOJANG", "BMCLAPI", ErrorMessage = "值只能为 MOJANG 或 BMCLAPI")]
            public string DownloadApi { get; set; }

            [Required(ErrorMessage = "配置项缺失")]
            public string GameVersion { get; set; }

            [Required(ErrorMessage = "配置项缺失")]
            [AllowedValues("CLIENT", "SERVER", ErrorMessage = "值只能为 CLIENT 或 SERVER")]
            public string InstanceType { get; set; }

            [Required(ErrorMessage = "配置项缺失")]
            [AllowedValues("RCON", "CONSOLE", "HYBRID", "MCAPI", ErrorMessage = "值只能为 RCON 或 CONSOLE 或 HYBRID 或 MCAPI")]
            public string CommunicationMode { get; set; }

            [Required(ErrorMessage = "配置项缺失")]
            public string MinecraftPath { get; set; }

            [Required(ErrorMessage = "配置项缺失")]
            public string ServerAddress { get; set; }

            [Required(ErrorMessage = "当配置项 CommunicationMode 的值为 CONSOLE 或 HYBRID 时，当前配置项的值不能为空")]
            public string JavaPath { get; set; }

            [Required(ErrorMessage = "当配置项 CommunicationMode 的值为 CONSOLE 或 HYBRID 时，当前配置项的值不能为空")]
            public string LaunchArguments { get; set; }

            [Range(0, 65535, ErrorMessage = "值的范围应该为0~65535")]
            public int McapiPort { get; set; }

            [Required(ErrorMessage = "配置项缺失")]
            public string Language { get; set; }

            [Required(ErrorMessage = "当配置项 CommunicationMode 的值为 MCAPI 时，当前配置项的值不能为空")]
            public string McapiPassword { get; set; }

            [Required(ErrorMessage = "配置项缺失")]
            public string[] ResourcePackList { get; set; }

            [Required(ErrorMessage = "配置项缺失")]
            public string[] BlockTextureBlacklist { get; set; }

#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        }
    }
}
