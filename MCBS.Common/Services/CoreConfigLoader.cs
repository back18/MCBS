using MCBS.Config;
using MCBS.Services;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace MCBS.Common.Services
{
    public class CoreConfigLoader
    {
        public CoreConfigLoader(
            IConfigPathProvider pathProvider,
            IConfigResourceProvider resourceProvider,
            [FromKeyedServices("MCBS.Common")] IFileFactory fileFactory)
        {
            ArgumentNullException.ThrowIfNull(pathProvider, nameof(pathProvider));
            ArgumentNullException.ThrowIfNull(resourceProvider, nameof(resourceProvider));
            ArgumentNullException.ThrowIfNull(fileFactory, nameof(fileFactory));

            _pathProvider = pathProvider;
            _resourceProvider = resourceProvider;
            _fileFactory = fileFactory;
        }

        private readonly IConfigPathProvider _pathProvider;
        private readonly IConfigResourceProvider _resourceProvider;
        private readonly IFileFactory _fileFactory;

        public void CreateIfNotExists()
        {
            CopyIfNotExists(_pathProvider.RegistryConfig, _resourceProvider.RegistryConfig);
            CopyIfNotExists(_pathProvider.Log4NetConfig, _resourceProvider.Log4NetConfig);
        }

        public CoreConfigManager.Model LoadAll()
        {
            Dictionary<string, string> registry = LoadJson(_pathProvider.RegistryConfig);

            return new CoreConfigManager.Model()
            {
                Registry = registry,
            };
        }

        private static Dictionary<string, string> LoadJson(FileInfo fileInfo)
        {
            using FileStream fileStream = fileInfo.OpenRead();
            using StreamReader streamReader = new(fileStream);
            string json = streamReader.ReadToEnd();
            var result = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            return result ?? [];
        }

        private void CopyIfNotExists(FileInfo fileInfo, string resourceName)
        {
            if (!fileInfo.Exists || fileInfo.Length == 0)
            {
                using FileStream fileStream = fileInfo.Create();
                using Stream stream = _fileFactory.CreateStream(resourceName);
                stream.CopyTo(fileStream);
                fileStream.Flush();
            }
        }
    }
}
