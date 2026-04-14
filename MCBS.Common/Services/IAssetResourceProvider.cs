using QuanLib.Minecraft.Downloading;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace MCBS.Common.Services
{
    public interface IAssetResourceProvider
    {
        public DownloadAsset GetDownloadAsset(AssetManifest assetManifest, string assetName);

        public bool TryGetDownloadAsset(AssetManifest assetManifest, [NotNullWhen(true)] string? assetName, [MaybeNullWhen(false)] out DownloadAsset result);
    }
}
