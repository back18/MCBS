﻿using QuanLib.Minecraft;
using QuanLib.Minecraft.Vector;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Screens
{
    public readonly struct ScreenOptions : IScreenOptions
    {
        public ScreenOptions(IScreenOptions options)
        {
            StartPosition = options.StartPosition;
            Width = options.Width;
            Height = options.Height;
            XFacing = options.XFacing;
            YFacing = options.YFacing;
        }

        public ScreenOptions(Model model)
        {
            StartPosition = new(model.StartPosition[0], model.StartPosition[1], model.StartPosition[2]);
            Width = model.Width;
            Height = model.Height;
            XFacing = (Facing)model.XFacing;
            YFacing = (Facing)model.YFacing;
        }

        public BlockPos StartPosition { get; }

        public int Width { get; }

        public int Height { get; }

        public Facing XFacing { get; }

        public Facing YFacing { get; }

        public Model ToModel()
        {
            return new Model
            {
                StartPosition = new int[] { StartPosition.X, StartPosition.Y, StartPosition.Z },
                Width = Width,
                Height = Height,
                XFacing = (int)XFacing,
                YFacing = (int)YFacing,
            };
        }

        public override string ToString()
        {
            return $"StartPosition={StartPosition}, Width={Width}, Height={Height}, XFacing={XFacing}, YFacing={YFacing}";
        }

        public static void Validate(string name, Model model)
        {
            ArgumentNullException.ThrowIfNull(model, nameof(model));

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

            [Required(ErrorMessage = "配置项缺失")]
            [Length(3, 3, ErrorMessage = "数组的长度应该为3")]
            public int[] StartPosition { get; set; }

            [Range(1, 512, ErrorMessage = "值的范围应该为1~512")]
            public int Width { get; set; }

            [Range(1, 512, ErrorMessage = "值的范围应该为1~512")]
            public int Height { get; set; }

            [AllowedValues(1, -1, 2, -2, 3, -3, ErrorMessage = "值只能为 1, -1, 2, -2, 3, -3")]
            public int XFacing { get; set; }

            [Range(-3, 3, ErrorMessage = "值的范围应该为-3~3")]
            public int YFacing { get; set; }
        }
    }
}
