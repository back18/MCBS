using MCBS.Config;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.Services
{
    public class ScreenConfigProvider : IScreenConfigProvider
    {
        public ScreenConfig Config => ConfigManager.ScreenConfig;
    }
}
