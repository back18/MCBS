using MCBS.Common.Services;
using MCBS.Config;
using MCBS.Config.Minecraft;
using MCBS.Drawing;
using MCBS.Drawing.Extensions;
using MCBS.Services;
using QuanLib.Core;
using QuanLib.Game;
using QuanLib.Minecraft.ResourcePack;
using QuanLib.Minecraft.ResourcePack.Block;
using QuanLib.Minecraft.ResourcePack.Language;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.ConsoleTerminal.Services
{
    public class MinecraftResourceLoader
    {
        public MinecraftResourceLoader(
            ILoggerProvider loggerProvider,
            IMinecraftConfigProvider minecraftConfigProvider,
            IScreenConfigProvider screenconfigProvider,
            ISystemConfigProvider systemConfigProvider,
            IColorMappingCacheLoader cacheLoader)
        {
            ArgumentNullException.ThrowIfNull(loggerProvider, nameof(loggerProvider));
            ArgumentNullException.ThrowIfNull(minecraftConfigProvider, nameof(minecraftConfigProvider));
            ArgumentNullException.ThrowIfNull(screenconfigProvider, nameof(screenconfigProvider));
            ArgumentNullException.ThrowIfNull(systemConfigProvider, nameof(systemConfigProvider));
            ArgumentNullException.ThrowIfNull(cacheLoader, nameof(cacheLoader));

            _logger = loggerProvider.GetLogger();
            _minecraftConfigProvider = minecraftConfigProvider;
            _screenconfigProvider = screenconfigProvider;
            _systemConfigProvider = systemConfigProvider;
            _cacheLoader = cacheLoader;
        }

        private readonly ILogger _logger;
        private readonly IMinecraftConfigProvider _minecraftConfigProvider;
        private readonly IScreenConfigProvider _screenconfigProvider;
        private readonly ISystemConfigProvider _systemConfigProvider;
        private readonly IColorMappingCacheLoader _cacheLoader;

        public async Task<MinecraftResourceManager.Model> LoadAsync(ResourceEntryManager resources)
        {
            MinecraftConfig minecraftConfig = _minecraftConfigProvider.Config;
            ScreenConfig screenConfig = _screenconfigProvider.Config;
            SystemConfig systemConfig = _systemConfigProvider.Config;

            LanguageManager languageManager = LanguageManager.LoadInstance(new(resources, minecraftConfig.Language));
            _logger.Info($"Minecraft语言数据加载完成，语言:{minecraftConfig.Language} 文本数量:{languageManager.Count}");

            using BlockTextureManager blockTextureManager = BlockTextureReader.Load(resources);
            var screenBlacklist = screenConfig.ScreenBlockBlacklist;
            BlockTexture[] blockTextures = blockTextureManager.GetBlockTextures(screenBlacklist);
            string[] blockIds = blockTextures.Select(s => s.BlockId).ToArray();
            Facing[] facings = Enum.GetValues<Facing>();

            _logger.Info($"Minecraft方块纹理数据加载完成，成功加载 {blockTextures.Length} 个方块的纹理数据（已剔除 {screenBlacklist.Count} 个黑名单方块状态）");

            HashBlockMapping hashBlockMapping = new(blockIds);
            Dictionary<Facing, Rgba32BlockMapping> rgba32BlockMappings = [];
            Dictionary<Facing, IColorMappingCache> colorMappingCaches = [];

            foreach (Facing facing in facings)
            {
                Rgba32BlockMapping rgba32BlockMapping = new(facing, blockTextures);
                rgba32BlockMappings.Add(facing, rgba32BlockMapping);
            }

            _logger.Info("Minecraft方块映射表构建完成");

            if (systemConfig.BuildColorMappingCaches)
            {
                foreach (Facing facing in facings)
                {
                    ColorFinder colorFinder = new(rgba32BlockMappings[facing].Colors);
                    IColorMappingCache colorMappingCache = await _cacheLoader.LoadAsync(facing, colorFinder).ConfigureAwait(false);
                    colorMappingCaches.Add(facing, colorMappingCache);
                }
                _logger.Info("Minecraft颜色映射表缓存构建完成");
            }
            else
            {
                foreach (Facing facing in facings)
                {
                    ColorFinder colorFinder = new(rgba32BlockMappings[facing].Colors);
                    ColorMappingTempCache colorMappingCache = new(colorFinder);
                    colorMappingCaches.Add(facing, colorMappingCache);
                }
                _logger.Info("Minecraft颜色映射表缓存已禁用，使用临时缓存");
            }

            return new MinecraftResourceManager.Model()
            {
                LanguageManager = languageManager,
                HashBlockMapping = hashBlockMapping,
                Rgba32BlockMappings = rgba32BlockMappings,
                ColorMappingCaches = colorMappingCaches
            };
        }
    }
}
