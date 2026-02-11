using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.Common.Services
{
    public interface IConfigResourceProvider
    {
        public string RegistryConfig { get; }

        public string Log4NetConfig { get; }
    }
}
