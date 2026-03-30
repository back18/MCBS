using MCBS.Config;
using QuanLib.Core;
using QuanLib.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace MCBS.WpfApp.Models
{
    public class MinecraftInstanceConfig : MinecraftConfig, IDataViewModel<MinecraftInstanceConfig>
    {
        public MinecraftInstanceConfig(Model model) : base(model)
        {
            ArgumentNullException.ThrowIfNull(model, nameof(model));

            InstanceName = model.InstanceName;
        }

        public string InstanceName { get; }

        public new static MinecraftInstanceConfig FromDataModel(object model)
        {
            return new MinecraftInstanceConfig((Model)model);
        }

        public override object ToDataModel()
        {
            return new Model()
            {
                InstanceName = InstanceName,
                MinecraftPath = MinecraftPath,
                MinecraftVersion = MinecraftVersion,
                IsServer = IsServer,
                ServerAddress = ServerAddress,
                ServerPort = ServerPort,
                Language = Language,
                ResourcePackList = ResourcePackList.ToArray(),
                DownloadSource = DownloadSource,
                CommunicationMode = CommunicationMode
            };
        }

        public new class Model : MinecraftConfig.Model, IDataModel<Model>
        {
            public Model() : base()
            {
                InstanceName = string.Empty;
            }

            public new static Model CreateDefault()
            {
                return new Model();
            }

            [Display(Order = -1, Name = "实例名称", Description = "用于标识Minecraft实例的名称")]
            [Required(ErrorMessage = ErrorMessageHelper.RequiredAttribute)]
            [FileName]
            public string InstanceName { get; set; }
        }
    }
}
