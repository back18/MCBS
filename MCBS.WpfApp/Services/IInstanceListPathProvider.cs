using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MCBS.WpfApp.Services
{
    public interface IInstanceListPathProvider
    {
        public FileInfo InstanceIndex { get; }

        public DirectoryInfo InstanceList { get; }

        public FileInfo GetInstanceConfig(string instanceName);
    }
}
