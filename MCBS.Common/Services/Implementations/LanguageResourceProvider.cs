using MCBS.Services;
using QuanLib.Minecraft.Downloading;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace MCBS.Common.Services.Implementations
{
    public class LanguageResourceProvider : IAssetResourceProvider
    {
        public LanguageResourceProvider(
            IMinecraftPathProvider pathProvider,
            IMinecraftDownloadProvider downloadProvider,
            ILanguageAssetMatchService assetMatchService)
        {
            ArgumentNullException.ThrowIfNull(pathProvider, nameof(pathProvider));
            ArgumentNullException.ThrowIfNull(downloadProvider, nameof(downloadProvider));
            ArgumentNullException.ThrowIfNull(assetMatchService, nameof(assetMatchService));

            _pathProvider = pathProvider;
            _downloadProvider = downloadProvider;
            _assetMatchService = assetMatchService;
        }

        private readonly IMinecraftPathProvider _pathProvider;
        private readonly IMinecraftDownloadProvider _downloadProvider;
        private readonly ILanguageAssetMatchService _assetMatchService;

        public DownloadAsset GetDownloadAsset(AssetManifest assetManifest, string assetName)
        {
            ArgumentNullException.ThrowIfNull(assetManifest, nameof(assetManifest));
            ArgumentException.ThrowIfNullOrEmpty(assetName, nameof(assetName));

            string[] candidates = _assetMatchService.Match(assetName);
            AssetIndex? assetIndex = null;
            foreach (string candidate in candidates)
            {
                if (assetManifest.TryGetValue(candidate, out assetIndex))
                {
                    assetName = candidate;
                    break;
                }
            }

            if (assetIndex is null)
                throw new KeyNotFoundException($"Asset '{assetName}' not found in manifest.");

            string url = _downloadProvider.GetAssetUrl(assetIndex.Hash);
            string folder = _pathProvider.Languages.FullName;
            string fileName = Path.GetFileName(assetName);

            return new DownloadAsset(url, assetIndex.HashType, assetIndex.Hash, assetIndex.Size, Path.Combine(folder, fileName));
        }

        public bool TryGetDownloadAsset(AssetManifest assetManifest, [NotNullWhen(true)] string? assetName, [MaybeNullWhen(false)] out DownloadAsset result)
        {
            ArgumentNullException.ThrowIfNull(assetManifest, nameof(assetManifest));
            if (string.IsNullOrEmpty(assetName))
            {
                result = null;
                return false;
            }

            string[] candidates = _assetMatchService.Match(assetName);
            AssetIndex? assetIndex = null;
            foreach (string candidate in candidates)
            {
                if (assetManifest.TryGetValue(candidate, out assetIndex))
                {
                    assetName = candidate;
                    break;
                }
            }

            if (assetIndex is null)
            {
                result = null;
                return false;
            }

            string url = _downloadProvider.GetAssetUrl(assetIndex.Hash);
            string folder = _pathProvider.Languages.FullName;
            string fileName = Path.GetFileName(assetName);

            result = new DownloadAsset(url, assetIndex.HashType, assetIndex.Hash, assetIndex.Size, Path.Combine(folder, fileName));
            return true;
        }
    }
}
