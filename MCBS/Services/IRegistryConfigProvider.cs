using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace MCBS.Services
{
    public interface IRegistryConfigProvider
    {
        public ReadOnlyDictionary<string, string> Config { get; }
    }
}
