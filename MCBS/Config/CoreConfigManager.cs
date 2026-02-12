using QuanLib.Core;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace MCBS.Config
{
    public static class CoreConfigManager
    {
        public static bool IsLoaded { get; private set; }

        public static ReadOnlyDictionary<string, string> Registry
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

            Registry = model.Registry.AsReadOnly();
        }

        public static void ApplyInitialize(this Model model)
        {
            ArgumentNullException.ThrowIfNull(model, nameof(model));

            Initialize(model);
        }

        private static T GetNotNull<T>(T? field, [CallerMemberName] string? propertyName = null) where T : class
        {
            return field ?? throw new InvalidOperationException($"属性“{propertyName}”未初始化");
        }

        public class Model
        {
            public required Dictionary<string, string> Registry { get; set; }
        }
    }
}
