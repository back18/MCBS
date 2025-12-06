using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.WpfApp.Config
{
    public interface IConfigProvider
    {
        public IConfigStorage GetSystemConfigService();

        public IConfigStorage GetMinecraftConfigService();

        public IConfigStorage GetScreenConfigService();
    }
}
