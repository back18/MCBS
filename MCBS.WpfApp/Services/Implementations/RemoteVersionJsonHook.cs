using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using QuanLib.Minecraft.Downloading;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MCBS.WpfApp.Services.Implementations
{
    public class RemoteVersionJsonHook(ILogger<RemoteVersionJsonHook>? logger) : DownloadCompletedHook<VersionJson>(logger)
    {
        private VersionJson? _versionJson;

        public override bool IsReady => _versionJson is not null || _isReady;

        public override async Task<VersionJson> LoadContentAsync()
        {
            await _semaphore.WaitAsync();
            try
            {
                ObjectDisposedException.ThrowIf(_isDisposed, this);

                if (_versionJson is not null)
                    return _versionJson;

                if (!_isReady || CloneStream is null)
                    throw new InvalidOperationException("No stream available to load the version JSON.");

                CloneStream.Seek(0, SeekOrigin.Begin);
                using StreamReader streamReader = new(CloneStream, Encoding.UTF8);
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
    }
}
