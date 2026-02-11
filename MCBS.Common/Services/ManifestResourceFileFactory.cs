using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace MCBS.Common.Services
{
    public class ManifestResourceFileFactory : IFileFactory
    {
        public ManifestResourceFileFactory(Assembly assembly, string? basePath = null)
        {
            ArgumentNullException.ThrowIfNull(assembly, nameof(assembly));

            StringBuilder stringBuilder = new();
            stringBuilder.Append(assembly.GetName().Name);
            stringBuilder.Append('.');
            if (!string.IsNullOrEmpty(basePath))
            {
                stringBuilder.Append(basePath);
                stringBuilder.Append('.');
            }

            _assembly = assembly;
            _basePath = stringBuilder.ToString();
        }

        private readonly Assembly _assembly;
        private readonly string _basePath;

        public Stream CreateStream(string? key = null)
        {
            ArgumentException.ThrowIfNullOrEmpty(key, nameof(key));

            string path = _basePath + key;
            return _assembly.GetManifestResourceStream(path) ?? throw new FileNotFoundException($"在程序集中找不到路径为“{path}”的嵌入文件", path);
        }
    }
}
