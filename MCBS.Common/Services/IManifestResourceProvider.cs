using QuanLib.Minecraft.Downloading;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.Common.Services
{
    public interface IManifestResourceProvider
    {
        public DownloadAsset GetDownloadAsset(VersionManifest versionManifest, string gameVersion);
    }
}
