using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using QuanLib.Minecraft.Downloading;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MCBS.WpfApp.Services.Implementations
{
    public class RemoteAssetManifestHook(ILogger<RemoteAssetManifestHook>? logger) : DownloadCompletedHook<AssetManifest>(logger)
    {
        private AssetManifest? _assetManifest;

        public override bool IsReady => _assetManifest is not null || _isReady;

        public override async Task<AssetManifest> LoadContentAsync()
        {
            await _semaphore.WaitAsync();
            try
            {
                ObjectDisposedException.ThrowIf(_isDisposed, this);

                if (_assetManifest is not null)
                    return _assetManifest;

                if (!_isReady || CloneStream is null)
                    throw new InvalidOperationException("No stream available to load the asset manifest.");

                CloneStream.Seek(0, SeekOrigin.Begin);
                using StreamReader streamReader = new(CloneStream, Encoding.UTF8);
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
    }
}
