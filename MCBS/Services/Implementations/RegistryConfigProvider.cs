using MCBS.Config;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace MCBS.Services.Implementations
{
    public class RegistryConfigProvider : IRegistryConfigProvider
    {
        public ReadOnlyDictionary<string, string> Config => CoreConfigManager.Registry;
    }
}
