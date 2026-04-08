using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.Services
{
    public interface IScopedMinecraftPathFactory
    {
        public IMinecraftPathProvider CreateProvider(string version);
    }
}
