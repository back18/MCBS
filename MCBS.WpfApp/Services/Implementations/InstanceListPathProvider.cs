using FFmpeg.AutoGen;
using MCBS.Services;
using QuanLib.IO.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MCBS.WpfApp.Services.Implementations
{
    public class InstanceListPathProvider : IInstanceListPathProvider
    {
        public InstanceListPathProvider(IMcbsPathProvider mcbsPathProvider)
        {
            ArgumentNullException.ThrowIfNull(mcbsPathProvider, nameof(mcbsPathProvider));

            _mcbsPathProvider = mcbsPathProvider;
        }

        private readonly IMcbsPathProvider _mcbsPathProvider;

        public FileInfo InstanceIndex => _mcbsPathProvider.Config.CombineFile("InstanceIndex.json");

        public DirectoryInfo InstanceList => _mcbsPathProvider.Config.CombineDirectory("InstanceList");

        public FileInfo GetInstanceConfig(string instanceName)
        {
            return _mcbsPathProvider.Config.CombineFile("InstanceList", instanceName + ".toml");
        }
    }
}
