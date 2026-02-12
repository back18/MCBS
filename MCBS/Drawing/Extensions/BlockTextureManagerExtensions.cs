using QuanLib.Minecraft;
using QuanLib.Minecraft.ResourcePack.Block;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.Drawing.Extensions
{
    public static class BlockTextureManagerExtensions
    {
        extension (BlockTextureManager source)
        {
            public BlockTexture[] GetBlockTextures()
            {
                return source.Values.ToArray();
            }

            public BlockTexture[] GetBlockTextures(IEnumerable<BlockState> blacklist)
            {
                ArgumentNullException.ThrowIfNull(blacklist, nameof(blacklist));

                List<BlockTexture> result = [];
                foreach (BlockTexture blockTexture in source.Values)
                {
                    if (!BlockState.TryParse(blockTexture.BlockId, out var blockState))
                        continue;

                    if (InBlacklist(blacklist, blockState))
                        continue;

                    result.Add(blockTexture);
                }

                return result.ToArray();
            }
        }

        private static bool InBlacklist(IEnumerable<BlockState> blacklist, BlockState blockState)
        {
            foreach (BlockState blacklistBlockState in blacklist)
            {
                if (blockState.BlockId != blacklistBlockState.BlockId)
                    continue;

                if (blacklistBlockState.States.Count == 0)
                    return true;

                foreach (var item in blacklistBlockState.States)
                {
                    if (blockState.States.TryGetValue(item.Key, out var value) && item.Value == value)
                        return true;
                }
            }

            return false;
        }
    }
}
