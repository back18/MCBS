﻿using QuanLib.Core;
using QuanLib.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Config.Minecraft
{
    public class ConsoleModeConfig : IDataModelOwner<ConsoleModeConfig, ConsoleModeConfig.Model>
    {
        private ConsoleModeConfig(Model model)
        {
            NullValidator.ValidateObject(model, nameof(model));

            JavaPath = model.JavaPath;
            LaunchArguments = model.LaunchArguments;
        }

        public string JavaPath { get; }

        public string LaunchArguments { get; }

        public static ConsoleModeConfig FromDataModel(Model model)
        {
            return new(model);
        }

        public Model ToDataModel()
        {
            return new()
            {
                JavaPath = JavaPath,
                LaunchArguments = LaunchArguments
            };
        }

        public class Model : IDataModel<Model>
        {
            public Model()
            {
                JavaPath = "java";
                LaunchArguments = string.Empty;
            }

            [Display(Name = "Java路径", Description = "启动Minecraft服务端进程所使用的Java路径")]
            [Required(ErrorMessage = ErrorMessageHelper.RequiredAttribute)]
            public string JavaPath { get; set; }

            [Display(Name = "启动参数", Description = "启动Minecraft服务端进程所使用的启动参数")]
            [Required(ErrorMessage = ErrorMessageHelper.RequiredAttribute)]
            public string LaunchArguments { get; set; }

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
