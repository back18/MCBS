using MCBS.Common.Services;
using MCBS.Services;
using MCBS.WpfApp.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using QuanLib.IO.Extensions;
using QuanLib.Minecraft.Downloading;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;

namespace MCBS.WpfApp.Services.Implementations
{
    public class GlobalInstanceResourceProvider : IGlobalInstanceResourceProvider
    {
        public GlobalInstanceResourceProvider(ILanguageAssetMatchService assetMatchService, IAsyncHashComputeService hashComputeService)
        {
            ArgumentNullException.ThrowIfNull(assetMatchService, nameof(assetMatchService));
            ArgumentNullException.ThrowIfNull(hashComputeService, nameof(hashComputeService));

            _assetMatchService = assetMatchService;
            _hashComputeService = hashComputeService;
        }

        private readonly ILanguageAssetMatchService _assetMatchService;
        private readonly IAsyncHashComputeService _hashComputeService;
        private IMinecraftPathProvider? _pathProvider;

        private NetworkAssetIndex? _indexFileAssetIndex;
        private NetworkAssetIndex? _clientCoreFileAssetIndex;
        private readonly Dictionary<string, AssetIndex> _languageFileAssets = [];

        public bool HasPathProvider => _pathProvider is not null;

        public void SetPathProvider(IMinecraftPathProvider pathProvider)
        {
            ArgumentNullException.ThrowIfNull(pathProvider, nameof(pathProvider));

            _pathProvider = pathProvider;
            _indexFileAssetIndex = null;
            _clientCoreFileAssetIndex = null;
            _languageFileAssets.Clear();
        }

        public GlobalFileModel GetVersionJsonFile()
        {
            ThrowIfPathProviderNotSet();

            FileInfo fileInfo = _pathProvider.VersionJson;
            if (!fileInfo.Exists || fileInfo.Length == 0)
                return new GlobalFileModel(fileInfo.Name, 0, GlobalFileStatus.NotFound);

            return new GlobalFileModel(fileInfo.Name, fileInfo.Length, GlobalFileStatus.Downloaded);
        }

        public async Task<GlobalFileModel> GetAssetManifestFileAsync(bool refresh = false)
        {
            ThrowIfPathProviderNotSet();

            FileInfo fileInfo = _pathProvider.IndexFile;
            if (!fileInfo.Exists || fileInfo.Length == 0)
                return new GlobalFileModel(fileInfo.Name, 0, GlobalFileStatus.NotFound);

            if (refresh || _indexFileAssetIndex is null)
            {
                VersionJson? versionJson = await TryReadVersionJsonAsync(_pathProvider);
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

        public async Task<GlobalFileModel> GetClientCoreFileAsync(bool refresh = false)
        {
            ThrowIfPathProviderNotSet();

            FileInfo fileInfo = _pathProvider.ClientCore;
            if (!fileInfo.Exists || fileInfo.Length == 0)
                return new GlobalFileModel(fileInfo.Name, 0, GlobalFileStatus.NotFound);

            if (refresh || _clientCoreFileAssetIndex is null)
            {
                VersionJson? versionJson = await TryReadVersionJsonAsync(_pathProvider);
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

        public async Task<GlobalFileModel> GetLanguageFileAsync(string language, bool refresh = false)
        {
            ArgumentException.ThrowIfNullOrEmpty(language, nameof(language));
            ThrowIfPathProviderNotSet();

            string fileName = language;
            if (!_pathProvider.Minecraft.Exists)
                return new GlobalFileModel(fileName, 0, GlobalFileStatus.NotFound);

            DirectoryInfo directoryInfo = _pathProvider.Languages;
            if (!directoryInfo.Exists)
                return new GlobalFileModel(fileName, 0, GlobalFileStatus.NotFound);

            FileInfo[] fileInfos = directoryInfo.GetFiles(language + ".*");
            if (fileInfos.Length == 0 || fileInfos.All(f => f.Length == 0))
                return new GlobalFileModel(fileName, 0, GlobalFileStatus.NotFound);

            AssetIndex? assetIndex = null;
            if (refresh || !_languageFileAssets.TryGetValue(language, out assetIndex))
            {
                AssetManifest? assetManifest = await TryReadAssetManifestAsync(_pathProvider);
                if (assetManifest is null)
                    return new GlobalFileModel(fileName, 0, GlobalFileStatus.VerifyFailed);

                string[] candidates = _assetMatchService.Match(language);
                foreach (string candidate in candidates)
                {
                    if (assetManifest.TryGetValue(candidate, out assetIndex))
                    {
                        fileName = Path.GetFileName(candidate);
                        break;
                    }
                }

                if (assetIndex is null)
                    return new GlobalFileModel(fileName, 0, GlobalFileStatus.VerifyFailed);

                _languageFileAssets[language] = assetIndex;
            }

            FileInfo fileInfo = _pathProvider.Languages.CombineFile(fileName);
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

        private static async Task<VersionJson?> TryReadVersionJsonAsync(IMinecraftPathProvider pathProvider)
        {
            try
            {
                if (pathProvider.VersionJson.ReadAllTextAsyncIfExists(out var task))
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

        private static async Task<AssetManifest?> TryReadAssetManifestAsync(IMinecraftPathProvider pathProvider)
        {
            try
            {
                if (pathProvider.IndexFile.ReadAllTextAsyncIfExists(out var task))
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

        [MemberNotNull(nameof(_pathProvider))]
        private void ThrowIfPathProviderNotSet()
        {
            if (_pathProvider is null)
                throw new InvalidOperationException("Path provider not set.");
        }
    }
}
