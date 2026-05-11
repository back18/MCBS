using QuanLib.Minecraft.Resource;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.Common.Services
{
    public interface IBlockTextureLoadService
    {
        public Task<BlockTextureManager> LoadBlockTextureAsync(ResourceEntryManager resourceEntryManager);
    }
}
