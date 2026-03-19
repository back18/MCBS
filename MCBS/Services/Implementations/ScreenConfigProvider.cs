using MCBS.Config;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.Services.Implementations
{
    public class ScreenConfigProvider : IScreenConfigProvider
    {
        public ScreenConfig Config => ConfigManager.ScreenConfig;
    }
}
