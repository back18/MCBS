using MCBS.Config;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.Services
{
    public class SystemConfigProvider : ISystemConfigProvider
    {
        public SystemConfig Config => ConfigManager.SystemConfig;
    }
}
