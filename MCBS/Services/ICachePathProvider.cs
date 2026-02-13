using QuanLib.Game;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.Services
{
    public interface ICachePathProvider
    {
        public DirectoryInfo Cache { get; }

        public DirectoryInfo ColorMapping { get; }

        public FileInfo GetColorMappingCache(Facing facing);

        public FileInfo GetColorMappingInfo(Facing facing);
    }
}
