using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.Services
{
    public interface IConfigPathProvider
    {
        public FileInfo MinecraftConfig { get; }

        public FileInfo SystemConfig { get; }

        public FileInfo ScreenConfig { get; }

        public FileInfo RegistryConfig { get; }

        public FileInfo Log4NetConfig { get; }
    }
}
