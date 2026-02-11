using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.Common.Services
{
    public class ConfigResourceProvider : IConfigResourceProvider
    {
        public string RegistryConfig { get; } = "Registry.json";

        public string Log4NetConfig { get; } = "log4net.xml";
    }
}
