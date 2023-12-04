﻿using Nett;
using Newtonsoft.Json;
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
                message.AppendLine($"[MinecraftPath]: Minecraft主目录路径“{model.MinecraftPath}”不存在");
                count++;
            }

            if (!(model.DownloadApi is DownloadApis.MOJANG or DownloadApis.BMCLAPI))
            {
                message.AppendLine("[DownloadApi]: Minecraft资源下载API只能为: MOJANG, BMCLAPI 中的其中之一");
                count++;
            }

            if (!(model.InstanceType is InstanceTypes.CLIENT or InstanceTypes.SERVER))
            {
                message.AppendLine("[InstanceType]: Minecraft实例类型只能为: CLIENT, SERVER 中的其中之一");
                count++;
            }

            if (!(model.CommunicationMode is CommunicationModes.CONSOLE or CommunicationModes.HYBRID or CommunicationModes.RCON or CommunicationModes.MCAPI))
            {
                message.AppendLine("[CommunicationMode]: Minecraft通信模式只能为 RCON, CONSOLE, HYBRID, MCAPI 中的其中之一");
                count++;
            }

            if (model.InstanceType == InstanceTypes.CLIENT && model.CommunicationMode != CommunicationModes.MCAPI)
            {
                message.AppendLine("[CommunicationMode]: 仅支持使用MCAPI与客户端进行通信");
                count++;
            }

            foreach (string resourcePack in model.ResourcePackList)
            {
                if (!SR.McbsDirectory.MinecraftDir.ResourcePacksDir.ExistsFile(resourcePack))
                {
                    message.AppendLine($"[ResourcePackList]: 资源包路径“{resourcePack}”不存在");
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

            [Required(ErrorMessage = "Minecraft资源下载API不能为空")]
            public string DownloadApi { get; set; }

            [Required(ErrorMessage = "Minecraft游戏版本不能为空")]
            public string GameVersion { get; set; }

            [Required(ErrorMessage = "Minecraft实例类型不能为空")]
            public string InstanceType { get; set; }

            [Required(ErrorMessage = "Minecraft通信模式不能为空")]
            public string CommunicationMode { get; set; }

            [Required(ErrorMessage = "Minecraft主目录路径不能为空")]
            public string MinecraftPath { get; set; }

            [Required(ErrorMessage = "服务器IP地址不能为空")]
            public string ServerAddress { get; set; }

            [Required(ErrorMessage = "当CommunicationMode为CONSOLE或HYBRID时，Java路径不能为空")]
            public string JavaPath { get; set; }

            [Required(ErrorMessage = "当CommunicationMode为CONSOLE或HYBRID时，启动参数不能为空")]
            public string LaunchArguments { get; set; }

            [Range(0, 65535, ErrorMessage = "端口范围应该在0到65535之间")]
            public int McapiPort { get; set; }

            [Required(ErrorMessage = "语言标识不能为空")]
            public string Language { get; set; }

            [Required(ErrorMessage = "当CommunicationMode为MCAPI时，MCAPI登录密码不能为空")]
            public string McapiPassword { get; set; }

            [Required(ErrorMessage = "配置项缺失")]
            public string[] ResourcePackList { get; set; }

            [Required(ErrorMessage = "配置项缺失")]
            public string[] BlockTextureBlacklist { get; set; }

#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
        }
    }
}
