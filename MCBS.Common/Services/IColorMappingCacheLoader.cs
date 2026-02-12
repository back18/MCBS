using MCBS.Drawing;
using QuanLib.Game;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.Common.Services
{
    public interface IColorMappingCacheLoader
    {
        public Task<IColorMappingCache> LoadAsync(Facing facing, IColorFinder colorFinder);
    }
}
