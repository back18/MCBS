using MCBS.Services;
using MCBS.WpfApp.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.WpfApp.Services
{
    public interface IGlobalInstanceResourceProvider
    {
        public bool HasPathProvider { get; }

        public void SetPathProvider(IMinecraftPathProvider pathProvider);

        public GlobalFileModel GetVersionJsonFile();

        public Task<GlobalFileModel> GetAssetManifestFileAsync(bool refresh = false);

        public Task<GlobalFileModel> GetClientCoreFileAsync(bool refresh = false);

        public Task<GlobalFileModel> GetLanguageFileAsync(string language, bool refresh = false);
    }
}
