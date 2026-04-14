using MCBS.Services;
using QuanLib.Minecraft.Downloading;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.Common.Services.Implementations
{
    public class IndexFileResourceProvider : IVersionResourceProvider
    {
        public IndexFileResourceProvider(IMinecraftPathProvider pathProvider, IMinecraftDownloadProvider downloadProvider)
        {
            ArgumentNullException.ThrowIfNull(pathProvider, nameof(pathProvider));
            ArgumentNullException.ThrowIfNull(downloadProvider, nameof(downloadProvider));

            _pathProvider = pathProvider;
            _downloadProvider = downloadProvider;
        }

        private readonly IMinecraftPathProvider _pathProvider;
        private readonly IMinecraftDownloadProvider _downloadProvider;

        public DownloadAsset GetDownloadAsset(VersionJson versionJson)
        {
            ArgumentNullException.ThrowIfNull(versionJson, nameof(versionJson));

            NetworkAssetIndex? assetIndex = versionJson.GetIndexFile()
                ?? throw new InvalidOperationException("Asset index resource not found.");

            string url = _downloadProvider.RedirectUrl(assetIndex.Url);
            return new DownloadAsset(url, assetIndex.HashType, assetIndex.Hash, assetIndex.Size, _pathProvider.IndexFile.FullName);
        }
    }
}
