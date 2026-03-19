using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.WpfApp.Services
{
    public interface IConfigProvider
    {
        public IConfigStorage GetSystemConfigService();

        public IConfigStorage GetMinecraftConfigService();

        public IConfigStorage GetScreenConfigService();
    }
}
