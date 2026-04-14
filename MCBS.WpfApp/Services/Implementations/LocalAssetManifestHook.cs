using MCBS.Services;
using Newtonsoft.Json;
using QuanLib.Minecraft.Downloading;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MCBS.WpfApp.Services.Implementations
{
    public class LocalAssetManifestHook : ILocalFileHook<AssetManifest>, IDisposable
    {
        public LocalAssetManifestHook(IMinecraftPathProvider pathProvider)
        {
            ArgumentNullException.ThrowIfNull(pathProvider, nameof(pathProvider));

            _pathProvider = pathProvider;
        }

        private readonly IMinecraftPathProvider _pathProvider;

        protected readonly SemaphoreSlim _semaphore = new(1);
        private AssetManifest? _assetManifest;

        public bool IsReady
        {
            get
            {
                FileInfo fileInfo = _pathProvider.IndexFile;
                return fileInfo.Exists && fileInfo.Length > 0;
            }
        }

        public async Task<AssetManifest> LoadContentAsync()
        {
            await _semaphore.WaitAsync();
            try
            {
                if (_assetManifest is not null)
                    return _assetManifest;

                using FileStream fileStream = _pathProvider.IndexFile.OpenRead();
                using StreamReader streamReader = new(fileStream, Encoding.UTF8);
                string json = await streamReader.ReadToEndAsync();
                var model = JsonConvert.DeserializeObject<AssetManifest.Model>(json) ?? throw new InvalidDataException();

                _assetManifest = new AssetManifest(model);
                return _assetManifest;
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
