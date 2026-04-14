using Downloader;
using MCBS.WpfApp.Events;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using QuanLib.Minecraft.Downloading;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MCBS.WpfApp.Services.Implementations
{
    public class RemoteVersionManifestHook(ILogger<RemoteVersionManifestHook>? logger) : DownloadCompletedHook<VersionManifest>(logger)
    {
        private VersionManifest? _versionManifest;

        public override bool IsReady => _versionManifest is not null || _isReady;

        public override async Task<VersionManifest> LoadContentAsync()
        {
            await _semaphore.WaitAsync();
            try
            {
                ObjectDisposedException.ThrowIf(_isDisposed, this);

                if (_versionManifest is not null)
                    return _versionManifest;

                if (!_isReady || CloneStream is null)
                    throw new InvalidOperationException("No stream available to load the version manifest.");

                CloneStream.Seek(0, SeekOrigin.Begin);
                using StreamReader streamReader = new(CloneStream, Encoding.UTF8);
                string json = await streamReader.ReadToEndAsync();
                var model = JsonConvert.DeserializeObject<VersionManifest.Model>(json) ?? throw new InvalidDataException();

                _versionManifest = new VersionManifest(model);
                return _versionManifest;
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }
}
