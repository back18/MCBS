using QuanLib.Minecraft.Downloading;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.Common.Services
{
    public interface IVersionResourceProvider
    {
        public DownloadAsset GetDownloadAsset(VersionJson versionJson);
    }
}
