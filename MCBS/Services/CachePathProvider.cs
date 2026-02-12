using QuanLib.Game;
using QuanLib.IO.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.Services
{
    public class CachePathProvider : ICachePathProvider
    {
        public DirectoryInfo Cache => McbsPathManager.MCBS_Cache;

        public DirectoryInfo ColorMapping => McbsPathManager.MCBS_Cache_ColorMapping;

        public FileInfo GetColorMappingCache(Facing facing)
        {
            return ColorMapping.CombineFile(facing.ToString() + ".bin");
        }

        public FileInfo GetColorMappingInfo(Facing facing)
        {
            return ColorMapping.CombineFile(facing.ToString() + ".json");
        }
    }
}
