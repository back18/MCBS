using MCBS.Drawing;
using QuanLib.Core;
using QuanLib.Game;
using QuanLib.Minecraft.ResourcePack.Language;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace MCBS
{
    public static class MinecraftResourceManager
    {
        public static bool IsLoaded { get; private set; }

        public static LanguageManager LanguageManager
        {
            get => GetNotNull(field);
            private set => field = value;
        }

        public static HashBlockMapping HashBlockMapping
        {
            get => GetNotNull(field);
            private set => field = value;
        }

        public static ReadOnlyDictionary<Facing, Rgba32BlockMapping> Rgba32BlockMappings
        {
            get => GetNotNull(field);
            private set => field = value;
        }

        public static ReadOnlyDictionary<Facing, IColorMappingCache> ColorMappingCaches
        {
            get => GetNotNull(field);
            private set => field = value;
        }

        public static void Initialize(Model model)
        {
            NullValidator.ValidateObject(model, nameof(model));

            if (IsLoaded)
                throw new InvalidOperationException("方块颜色映射管理器完成已初始化，无法重复初始化");
            IsLoaded = true;

            HashBlockMapping = model.HashBlockMapping;
            Rgba32BlockMappings = model.Rgba32BlockMappings.AsReadOnly();
            ColorMappingCaches = model.ColorMappingCaches.AsReadOnly();
        }

        public static IColorFinder CreateColorFinder(Facing facing)
        {
            return new ColorFinder(Rgba32BlockMappings[facing].Colors);
        }

        public static ColorMatcher<TPixel> CreateColorMatcher<TPixel>(Facing facing) where TPixel : unmanaged, IPixel<TPixel>
        {
            IColorFinder colorFinder = CreateColorFinder(facing);
            if (!ColorMappingCaches.TryGetValue(facing, out var mappingCache))
                mappingCache = new ColorMappingTempCache(colorFinder);
            return new ColorMatcher<TPixel>(colorFinder, mappingCache);
        }

        private static T GetNotNull<T>(T? field, [CallerMemberName] string? propertyName = null) where T : class
        {
            return field ?? throw new InvalidOperationException($"属性“{propertyName}”初始化");
        }

        public static void ApplyInitialize(this Model model)
        {
            ArgumentNullException.ThrowIfNull(model, nameof(model));

            Initialize(model);
        }

        public class Model
        {
            public required LanguageManager LanguageManager { get; set; }

            public required HashBlockMapping HashBlockMapping { get; set; }

            public required Dictionary<Facing, Rgba32BlockMapping> Rgba32BlockMappings { get; set; }

            public required Dictionary<Facing, IColorMappingCache> ColorMappingCaches { get; set; }
        }
    }
}
