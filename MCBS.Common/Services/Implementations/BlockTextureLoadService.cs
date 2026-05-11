using QuanLib.Minecraft.Resource;
using QuanLib.Minecraft.Resource.Models;
using QuanLib.Minecraft.Resource.Services;
using QuanLib.Minecraft.Resource.Textures;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.Common.Services.Implementations
{
    public class BlockTextureLoadService : IBlockTextureLoadService
    {
        public BlockTextureLoadService(IBlockStateLoader blockStateLoader, IBlockModelLoader blockModelLoader, IBlockTextureLoader blockTextureLoader)
        {
            ArgumentNullException.ThrowIfNull(blockStateLoader, nameof(blockStateLoader));
            ArgumentNullException.ThrowIfNull(blockModelLoader, nameof(blockModelLoader));
            ArgumentNullException.ThrowIfNull(blockTextureLoader, nameof(blockTextureLoader));

            _blockStateLoader = blockStateLoader;
            _blockModelLoader = blockModelLoader;
            _blockTextureLoader = blockTextureLoader;
        }

        private readonly IBlockStateLoader _blockStateLoader;
        private readonly IBlockModelLoader _blockModelLoader;
        private readonly IBlockTextureLoader _blockTextureLoader;

        public async Task<BlockTextureManager> LoadBlockTextureAsync(ResourceEntryManager resourceEntryManager)
        {
            ArgumentNullException.ThrowIfNull(resourceEntryManager, nameof(resourceEntryManager));

            IEnumerable<AssetFileEntry> blockStateEntries = GetAllBlockStates(resourceEntryManager);
            Dictionary<string, AssetFileEntry> blockModelEntries = GetAllBlockModels(resourceEntryManager);
            Dictionary<string, AssetFileEntry> blockTextureEntries = GetAllBlockTextures(resourceEntryManager);
            TexturePool texturePool = new();

            Dictionary<string, BlockStateModel> blockStates = await _blockStateLoader.LoadBlockStatesAsync(blockStateEntries);
            Dictionary<string, ICubeBlockModel> blockModels = await _blockModelLoader.LoadBlockModelsAsync(blockModelEntries, blockStates);
            Dictionary<string, ICubeBlockTexture> blockTextures = await _blockTextureLoader.LoadBlockTexturesAsync(blockTextureEntries, blockModels, texturePool);

            return BlockTextureManager.LoadInstance(new(blockTextures, texturePool));
        }

        private static IEnumerable<AssetFileEntry> GetAllBlockStates(ResourceEntryManager resourceEntryManager)
        {
            IEnumerable<AssetFileEntry> result = Enumerable.Empty<AssetFileEntry>();
            foreach (ResourceEntry entry in resourceEntryManager.Values)
            {
                result = result.Concat(entry.BlockStates.Values);
            }

            return result;
        }

        private static Dictionary<string, AssetFileEntry> GetAllBlockModels(ResourceEntryManager resourceEntryManager)
        {
            Dictionary<string, AssetFileEntry> result = [];
            foreach (ResourceEntry entry in resourceEntryManager.Values)
            {
                foreach (var item in entry.BlockModels)
                    result[item.Key] = item.Value;
            }

            return result;
        }

        private static Dictionary<string, AssetFileEntry> GetAllBlockTextures(ResourceEntryManager resourceEntryManager)
        {
            Dictionary<string, AssetFileEntry> result = [];
            foreach (ResourceEntry entry in resourceEntryManager.Values)
            {
                foreach (var item in entry.BlockTextures)
                    result[item.Key] = item.Value;
            }

            return result;
        }
    }
}
