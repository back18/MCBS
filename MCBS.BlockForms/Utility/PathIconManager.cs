using MCBS.Config;
using QuanLib.Core;
using QuanLib.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.BlockForms.Utility
{
    public static class PathIconManager
    {
        public static Image<Rgba32> GetIcon(string path)
        {
            ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));

            PathType pathType = FileUtil.GetPathType(path);

            return pathType switch
            {
                PathType.Unknown => TextureManager.Instance["FileErrorIcon"],
                PathType.Drive => TextureManager.Instance["DriverIcon"],
                PathType.Directory => TextureManager.Instance["FolderIcon"],
                PathType.File => GetFileIcon(path),
                _ => throw new InvalidEnumArgumentException(),
            };
        }

        public static Image<Rgba32> GetFileIcon(string path)
        {
            if (!File.Exists(path))
                return TextureManager.Instance["FileErrorIcon"];

            string extension = Path.GetExtension(path).TrimStart('.');
            if (!CoreConfigManager.Registry.TryGetValue(extension, out var appId) ||
                !MinecraftBlockScreen.Instance.AppComponents.TryGetValue(appId, out var applicationManifest))
                return TextureManager.Instance["FileIcon"];

            return applicationManifest.GetIcon();
        }
    }
}
