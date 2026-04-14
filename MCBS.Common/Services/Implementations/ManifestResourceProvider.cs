using MCBS.Services;
using QuanLib.Core;
using QuanLib.IO;
using QuanLib.Minecraft.Downloading;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.Common.Services.Implementations
{
    public class ManifestResourceProvider : IManifestResourceProvider
    {
        public ManifestResourceProvider(IMinecraftPathProvider pathProvider, IMinecraftDownloadProvider downloadProvider)
        {
            ArgumentNullException.ThrowIfNull(pathProvider, nameof(pathProvider));
            ArgumentNullException.ThrowIfNull(downloadProvider, nameof(downloadProvider));

            _pathProvider = pathProvider;
            _downloadProvider = downloadProvider;
        }

        private readonly IMinecraftPathProvider _pathProvider;
        private readonly IMinecraftDownloadProvider _downloadProvider;

        public DownloadAsset GetDownloadAsset(VersionManifest versionManifest, string gameVersion)
        {
            ArgumentNullException.ThrowIfNull(versionManifest, nameof(versionManifest));
            ArgumentException.ThrowIfNullOrEmpty(gameVersion, nameof(gameVersion));
            if (!versionManifest.TryGetValue(gameVersion, out var versionIndex))
                throw new KeyNotFoundException($"Game version '{gameVersion}' not found in manifest.");

            string url = _downloadProvider.RedirectUrl(versionIndex.Url);
            string hash = GetHashFromUrl(url);

            return new DownloadAsset(url, HashType.SHA1, hash, -1, _pathProvider.VersionJson.FullName);
        }

        private static string GetHashFromUrl(string url)
        {
            Uri uri = new(url);
            if (uri.Segments.Length < 2)
                return string.Empty;

            string hash = uri.Segments[^2].TrimEnd('/');
            if (hash.Length != HashType.SHA1.GetHashSizeInChars())
                return string.Empty;

            return hash;
        }
    }
}
