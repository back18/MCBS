using MCBS.Config.Minecraft;
using QuanLib.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Config
{
    public static class ConfigManager
    {
        public static bool IsLoaded { get; private set; }

        public static MinecraftConfig MinecraftConfig
        {
            get => GetNotNull(field);
            private set => field = value;
        }

        public static SystemConfig SystemConfig
        {
            get => GetNotNull(field);
            private set => field = value;
        }

        public static ScreenConfig ScreenConfig
        {
            get => GetNotNull(field);
            private set => field = value;
        }

        public static void Initialize(Model model)
        {
            NullValidator.ValidateObject(model, nameof(model));

            if (IsLoaded)
                throw new InvalidOperationException("配置管理器完成已初始化，无法重复初始化");
            IsLoaded = true;

            MinecraftConfig = model.MinecraftConfig;
            SystemConfig = model.SystemConfig;
            ScreenConfig = model.ScreenConfig;
        }

        private static T GetNotNull<T>(T? field, [CallerMemberName] string? propertyName = null) where T : class
        {
            return field ?? throw new InvalidOperationException($"属性“{propertyName}”未初始化");
        }

        public static void ApplyInitialize(this Model model)
        {
            ArgumentNullException.ThrowIfNull(model, nameof(model));

            Initialize(model);
        }

        public class Model
        {
            public required MinecraftConfig MinecraftConfig { get; set; }

            public required SystemConfig SystemConfig { get; set; }

            public required ScreenConfig ScreenConfig { get; set; }
        }
    }
}
