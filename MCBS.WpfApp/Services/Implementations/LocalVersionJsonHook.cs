using MCBS.Services;
using Newtonsoft.Json.Linq;
using QuanLib.Minecraft.Downloading;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MCBS.WpfApp.Services.Implementations
{
    public class LocalVersionJsonHook : ILocalFileHook<VersionJson>, IDisposable
    {
        public LocalVersionJsonHook(IMinecraftPathProvider pathProvider)
        {
            ArgumentNullException.ThrowIfNull(pathProvider, nameof(pathProvider));

            _pathProvider = pathProvider;
        }

        private readonly IMinecraftPathProvider _pathProvider;

        protected readonly SemaphoreSlim _semaphore = new(1);
        private VersionJson? _versionJson;

        public bool IsReady
        {
            get
            {
                FileInfo fileInfo = _pathProvider.VersionJson;
                return fileInfo.Exists && fileInfo.Length > 0;
            }
        }

        public async Task<VersionJson> LoadContentAsync()
        {
            await _semaphore.WaitAsync();
            try
            {
                if (_versionJson is not null)
                    return _versionJson;

                using FileStream fileStream = _pathProvider.VersionJson.OpenRead();
                using StreamReader streamReader = new(fileStream, Encoding.UTF8);
                string json = await streamReader.ReadToEndAsync();
                JObject jobj = JObject.Parse(json);

                _versionJson = new VersionJson(jobj);
                return _versionJson;
            }
            finally
            {
                _semaphore.Release(); 
            }
        }

        public void Dispose()
        {
            _semaphore.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
