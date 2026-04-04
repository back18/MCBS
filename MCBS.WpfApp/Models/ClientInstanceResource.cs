using Newtonsoft.Json;
using QuanLib.Minecraft;
using QuanLib.Minecraft.Downloading;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using static QuanLib.Minecraft.Downloading.GameAssetHelper;

namespace MCBS.WpfApp.Models
{
    public class ClientInstanceResource
    {
        public ClientInstanceResource(string gameFolder, string gameVersion)
        {
            ArgumentException.ThrowIfNullOrEmpty(gameFolder, nameof(gameFolder));
            ArgumentException.ThrowIfNullOrEmpty(gameVersion, nameof(gameVersion));

            GameFolder = Path.GetFullPath(gameFolder);
            GameVersion = gameVersion;
        }

        public string GameFolder { get; }

        public string GameVersion { get; }

        public string? VersionFolder { get; private set; }

        public string? AssetFolder { get; private set; }

        public string? ResourcePackFolder { get; private set; }

        public string? VersionJsonFile { get; private set; }

        public string? AssetManifestFile { get; private set; }

        public string? ClientCoreFile { get; private set; }

        public string? ClientOptionsFile { get; private set; }

        public VersionJson? VersionJson { get; private set; }

        public AssetManifest? AssetManifest { get; private set; }

        public AssetManifestTree? AssetManifestTree { get; private set; }

        public AssetObjectStorage? AssetObjectStorage { get; private set; }

        public ClientOptions? ClientOptions { get; private set; }

        public async Task Initialize()
        {
            if (string.IsNullOrEmpty(VersionJsonFile) || VersionJson is null)
            {
                VersionJsonFile? versionJsonFile = await QueryVersionJsonFileAsync(GameFolder, GameVersion);
                if (versionJsonFile is null)
                    return;

                VersionJsonFile = versionJsonFile.FilePath;
                VersionJson = versionJsonFile.VersionJson;
            }

            VersionFolder = Path.GetDirectoryName(VersionJsonFile);
            AssetFolder = GetAssetFolder(VersionJsonFile);

            if (string.IsNullOrEmpty(VersionFolder) || string.IsNullOrEmpty(AssetFolder))
                return;

            string assetManifestId = VersionJson.Assets;
            AssetManifestFile = Path.Combine(AssetFolder, "indexes", assetManifestId + ".json");

            string clientCoreNmae = VersionJson.Jar + ".jar";
            ClientCoreFile = Path.Combine(VersionFolder, clientCoreNmae);

            ResourcePackFolder = GetResourcePackFolder(VersionJsonFile);
            ClientOptionsFile = GetClientOptionsFile(VersionJsonFile);

            try
            {
                await LoadAssetManifest();
                await LoadClientOptions();
            }
            catch
            {
                //TODO: log error
            }
        }

        public IEnumerable<string> GetResourcePacks()
        {
            if (!Directory.Exists(ResourcePackFolder))
                return Array.Empty<string>();

            return Directory.GetFiles(ResourcePackFolder, "*.zip").OrderBy(f => Path.GetFileName(f));
        }

        public IEnumerable<AssetFileModel> GetLanguageFiles()
        {
            if (string.IsNullOrEmpty(AssetFolder) || string.IsNullOrEmpty(AssetManifestFile))
                return Array.Empty<AssetFileModel>();

            if (AssetManifest is null || AssetManifestTree is null || AssetObjectStorage is null)
                return Array.Empty<AssetFileModel>();

            try
            {
                string[] languageFiles = AssetManifestTree.GetFilePaths("minecraft/lang");
                return languageFiles.Order().Select(f =>
                    new AssetFileModel(new FileInfo(AssetManifestTree.GetStoragePath(f)), f, Path.GetFileName(f)));
            }
            catch
            {
                return Array.Empty<AssetFileModel>();
            }
        }

        public IEnumerable<string> GetSelectedResourcePacks()
        {
            if (string.IsNullOrEmpty(ClientOptionsFile))
                return Array.Empty<string>();

            if (ClientOptions is null)
                return Array.Empty<string>();

            var selectedPacks = ClientOptions.ResourcePacks;
            string prefix = "file/";
            return selectedPacks.Where(s => s.StartsWith(prefix)).Select(s => s[prefix.Length..]);
        }

        public string GetSelectedLanguageName()
        {
            if (string.IsNullOrEmpty(ClientOptionsFile))
                return string.Empty;

            if (ClientOptions is null)
                return string.Empty;

            return ClientOptions.Lang;
        }

        private async Task LoadAssetManifest()
        {
            if (string.IsNullOrEmpty(AssetFolder) || string.IsNullOrEmpty(AssetManifestFile))
                throw new InvalidOperationException("Unable to load asset manifest, asset folder or manifest file is not available.");

            AssetManifest = await LoadAssetManifestAsync(AssetManifestFile);
            string objectsFolder = Path.Combine(AssetFolder, "objects");
            AssetObjectStorage = new(objectsFolder, AssetManifest.Values.First().HashType);
            AssetManifestTree = new(AssetObjectStorage, AssetManifest);
        }

        private async Task<ClientOptions> LoadClientOptions()
        {
            if (string.IsNullOrEmpty(ClientOptionsFile))
                throw new InvalidOperationException("Unable to load client options, options file is not available.");

            string clientOptions = await File.ReadAllTextAsync(ClientOptionsFile, Encoding.UTF8);
            Dictionary<string, string> options = ClientOptions.Parse(clientOptions);
            ClientOptions = ClientOptions.Load(options);
            return ClientOptions;
        }

        private static async Task<AssetManifest> LoadAssetManifestAsync(string assetManifestFile)
        {
            string json = await File.ReadAllTextAsync(assetManifestFile, Encoding.UTF8);
            var model = JsonConvert.DeserializeObject<AssetManifest.Model>(json) ?? throw new InvalidDataException();
            return new AssetManifest(model);
        }
    }
}
