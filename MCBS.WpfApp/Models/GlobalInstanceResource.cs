using MCBS.Common.Services;
using MCBS.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QuanLib.IO.Extensions;
using QuanLib.Minecraft.Downloading;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MCBS.WpfApp.Models
{
    public class GlobalInstanceResource
    {
        public GlobalInstanceResource(IMinecraftPathProvider pathProvider, IAsyncHashComputeService hashComputeService)
        {
            ArgumentNullException.ThrowIfNull(pathProvider, nameof(pathProvider));
            ArgumentNullException.ThrowIfNull(hashComputeService, nameof(hashComputeService));

            _pathProvider = pathProvider;
            _hashComputeService = hashComputeService;
        }

        private readonly IMinecraftPathProvider _pathProvider;
        private readonly IAsyncHashComputeService _hashComputeService;

        private NetworkAssetIndex? _indexFileAssetIndex;
        private NetworkAssetIndex? _clientCoreFileAssetIndex;
        private Dictionary<string, AssetIndex> _languageFileAssetIndex = [];

        public GlobalFileModel GetVersionJsonFile()
        {
            FileInfo fileInfo = _pathProvider.VersionJson;

            if (!fileInfo.Exists || fileInfo.Length == 0)
                return new GlobalFileModel(fileInfo.Name, 0, GlobalFileStatus.NotFound);

            return new GlobalFileModel(fileInfo.Name, fileInfo.Length, GlobalFileStatus.Downloaded);
        }

        public async Task<GlobalFileModel> GetAssetManifestFileAsync()
        {
            FileInfo fileInfo = _pathProvider.IndexFile;

            if (!fileInfo.Exists || fileInfo.Length == 0)
                return new GlobalFileModel(fileInfo.Name, 0, GlobalFileStatus.NotFound);

            if (_indexFileAssetIndex is null)
            {
                VersionJson? versionJson = await TryReadVersionJsonAsync();
                if (versionJson is null)
                    return new GlobalFileModel(fileInfo.Name, fileInfo.Length, GlobalFileStatus.VerifyFailed);

                _indexFileAssetIndex = versionJson.GetIndexFile();
                _clientCoreFileAssetIndex = versionJson.GetClientCore();
            }

            if (_indexFileAssetIndex is null)
                return new GlobalFileModel(fileInfo.Name, fileInfo.Length, GlobalFileStatus.VerifyFailed);

            bool valid = await ValidateFileAsync(_indexFileAssetIndex, fileInfo);
            if (!valid)
                return new GlobalFileModel(fileInfo.Name, fileInfo.Length, GlobalFileStatus.VerifyFailed);

            return new GlobalFileModel(fileInfo.Name, fileInfo.Length, GlobalFileStatus.Downloaded);
        }

        public async Task<GlobalFileModel> GetClientCoreFileAsync()
        {
            FileInfo fileInfo = _pathProvider.ClientCore;

            if (!fileInfo.Exists || fileInfo.Length == 0)
                return new GlobalFileModel(fileInfo.Name, 0, GlobalFileStatus.NotFound);

            if (_clientCoreFileAssetIndex is null)
            {
                VersionJson? versionJson = await TryReadVersionJsonAsync();
                if (versionJson is null)
                    return new GlobalFileModel(fileInfo.Name, fileInfo.Length, GlobalFileStatus.VerifyFailed);

                _indexFileAssetIndex = versionJson.GetIndexFile();
                _clientCoreFileAssetIndex = versionJson.GetClientCore();
            }

            if (_clientCoreFileAssetIndex is null)
                return new GlobalFileModel(fileInfo.Name, fileInfo.Length, GlobalFileStatus.VerifyFailed);

            bool valid = await ValidateFileAsync(_clientCoreFileAssetIndex, fileInfo);
            if (!valid)
                return new GlobalFileModel(fileInfo.Name, fileInfo.Length, GlobalFileStatus.VerifyFailed);

            return new GlobalFileModel(fileInfo.Name, fileInfo.Length, GlobalFileStatus.Downloaded);
        }

        public async Task<GlobalFileModel> GetLanguageFileAsync(string language)
        {
            ArgumentException.ThrowIfNullOrEmpty(language, nameof(language));

            string langFileName = language + ".json";
            FileInfo fileInfo = _pathProvider.Languages.CombineFile(langFileName);

            if (!fileInfo.Exists || fileInfo.Length == 0)
                return new GlobalFileModel(fileInfo.Name, 0, GlobalFileStatus.NotFound);

            if (!_languageFileAssetIndex.TryGetValue(language, out var assetIndex))
            {
                AssetManifest? assetManifest = await TryReadAssetManifestAsync();
                if (assetManifest is null)
                    return new GlobalFileModel(fileInfo.Name, fileInfo.Length, GlobalFileStatus.VerifyFailed);

                string langAssetPath = "minecraft/lang/" + langFileName;
                if (!assetManifest.TryGetValue(langAssetPath, out assetIndex))
                    return new GlobalFileModel(fileInfo.Name, fileInfo.Length, GlobalFileStatus.VerifyFailed);

                _languageFileAssetIndex[language] = assetIndex;
            }

            bool valid = await ValidateFileAsync(assetIndex, fileInfo);
            if (!valid)
                return new GlobalFileModel(fileInfo.Name, fileInfo.Length, GlobalFileStatus.VerifyFailed);

            return new GlobalFileModel(fileInfo.Name, fileInfo.Length, GlobalFileStatus.Downloaded);
        }

        private async Task<bool> ValidateFileAsync(AssetIndex assetIndex, FileInfo fileInfo)
        {
            if (!fileInfo.Exists)
                return false;

            if (fileInfo.Length != assetIndex.Size)
                return false;

            using FileStream fileStream = fileInfo.OpenRead();
            string hash = await _hashComputeService.GetHashStringAsync(fileStream, assetIndex.HashType);
            return string.Equals(hash, assetIndex.Hash, StringComparison.OrdinalIgnoreCase);
        }

        private async Task<VersionJson?> TryReadVersionJsonAsync()
        {
            try
            {
                if (_pathProvider.VersionJson.ReadAllTextAsyncIfExists(out var task))
                {
                    string json = await task;
                    return new VersionJson(JObject.Parse(json));
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

        private async Task<AssetManifest?> TryReadAssetManifestAsync()
        {
            try
            {
                if (_pathProvider.IndexFile.ReadAllTextAsyncIfExists(out var task))
                {
                    string json = await task;
                    var model = JsonConvert.DeserializeObject<AssetManifest.Model>(json);
                    if (model is null)
                        return null;

                    return new AssetManifest(model);
                }
                else
                {
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
