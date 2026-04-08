using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.Services.Implementations
{
    public class ScopedMinecraftPathFactory : IScopedMinecraftPathFactory
    {
        public IMinecraftPathProvider CreateProvider(string version)
        {
            return new ScopedMinecraftPathProvider(version);
        }
    }
}
