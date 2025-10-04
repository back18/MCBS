using MCBS.Config;
using QuanLib.IO.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MCBS
{
    public static class McbsPathManager
    {
        private static string MinecraftVersion => ConfigManager.MinecraftConfig.MinecraftVersion;

        public static DirectoryInfo MCBS => Paths.MCBS.CreateDirectoryInfo();

        public static DirectoryInfo MCBS_Configs => Paths.MCBS_Configs.CreateDirectoryInfo();

        public static FileInfo MCBS_Configs_MinecraftConfig => Paths.MCBS_Configs_MinecraftConfig.CreateFileInfo();

        public static FileInfo MCBS_Configs_SystemConfig => Paths.MCBS_Configs_SystemConfig.CreateFileInfo();

        public static FileInfo MCBS_Configs_ScreenConfig => Paths.MCBS_Configs_ScreenConfig.CreateFileInfo();

        public static FileInfo MCBS_Configs_RegistryConfig => Paths.MCBS_Configs_RegistryConfig.CreateFileInfo();

        public static FileInfo MCBS_Configs_Log4NetConfig => Paths.MCBS_Configs_Log4NetConfig.CreateFileInfo();

        public static DirectoryInfo MCBS_Caches => Paths.MCBS_Caches.CreateDirectoryInfo();

        public static DirectoryInfo MCBS_Caches_ColorMapping => Paths.MCBS_Caches_ColorMapping.CreateDirectoryInfo();

        public static DirectoryInfo MCBS_Applications => Paths.MCBS_Applications.CreateDirectoryInfo();

        public static DirectoryInfo MCBS_DllAppComponents => Paths.MCBS_DllAppComponents.CreateDirectoryInfo();

        public static DirectoryInfo MCBS_Logs => Paths.MCBS_Logs.CreateDirectoryInfo();

        public static FileInfo MCBS_Logs_LatestLog => Paths.MCBS_Logs_LatestLog.CreateFileInfo();

        public static DirectoryInfo MCBS_Minecraft => Paths.MCBS_Minecraft.CreateDirectoryInfo();

        public static DirectoryInfo MCBS_Minecraft_ResourcePacks => Paths.MCBS_Minecraft_ResourcePacks.CreateDirectoryInfo();

        public static DirectoryInfo MCBS_Minecraft_Vanilla => Paths.MCBS_Minecraft_Vanilla.CreateDirectoryInfo();

        [CancelCreate]
        public static DirectoryInfo MCBS_Minecraft_Vanilla_Version => string.Format(Paths.MCBS_Minecraft_Vanilla_Version, MinecraftVersion).CreateDirectoryInfo();

        [CancelCreate]
        public static DirectoryInfo MCBS_Minecraft_Vanilla_Version_Languages => string.Format(Paths.MCBS_Minecraft_Vanilla_Version_Languages, MinecraftVersion).CreateDirectoryInfo();

        [CancelCreate]
        public static FileInfo MCBS_Minecraft_Vanilla_Version_VersionJson => string.Format(Paths.MCBS_Minecraft_Vanilla_Version_VersionJson, MinecraftVersion).CreateFileInfo();

        [CancelCreate]
        public static FileInfo MCBS_Minecraft_Vanilla_Version_ClientCore => string.Format(Paths.MCBS_Minecraft_Vanilla_Version_ClientCore, MinecraftVersion).CreateFileInfo();

        [CancelCreate]
        public static FileInfo MCBS_Minecraft_Vanilla_Version_IndexFile => string.Format(Paths.MCBS_Minecraft_Vanilla_Version_IndexFile, MinecraftVersion).CreateFileInfo();

        public static DirectoryInfo MCBS_FFmpeg => Paths.MCBS_FFmpeg.CreateDirectoryInfo();

        public static DirectoryInfo MCBS_FFmpeg_Bin => Paths.MCBS_FFmpeg_Bin.CreateDirectoryInfo();

        public static FileInfo MCBS_FFmpeg_Win64ZipFile => Paths.MCBS_FFmpeg_Win64ZipFile.CreateFileInfo();

        public static FileInfo MCBS_FFmpeg_Win64IndexFile => Paths.MCBS_FFmpeg_Win64IndexFile.CreateFileInfo();

        public static void CreateAllDirectory()
        {
            Type type = typeof(McbsPathManager);

            foreach (PropertyInfo propertyInfo in type.GetProperties(BindingFlags.Public | BindingFlags.Static))
            {
                if (propertyInfo.PropertyType != typeof(DirectoryInfo) || propertyInfo.GetCustomAttribute<CancelCreateAttribute>() is not null || propertyInfo.GetValue(null) is not DirectoryInfo directoryInfo)
                    continue;

                if (!directoryInfo.Exists)
                    directoryInfo.Create();
            }
        }

        private class Paths
        {
            public static readonly string MCBS = Environment.CurrentDirectory.PathCombine("MCBS");

            public static readonly string MCBS_Configs = MCBS.PathCombine("Configs");

            public static readonly string MCBS_Configs_MinecraftConfig = MCBS_Configs.PathCombine("Minecraft.toml");

            public static readonly string MCBS_Configs_SystemConfig = MCBS_Configs.PathCombine("System.toml");

            public static readonly string MCBS_Configs_ScreenConfig = MCBS_Configs.PathCombine("Screen.toml");

            public static readonly string MCBS_Configs_RegistryConfig = MCBS_Configs.PathCombine("Registry.json");

            public static readonly string MCBS_Configs_Log4NetConfig = MCBS_Configs.PathCombine("log4net.xml");

            public static readonly string MCBS_Caches = MCBS.PathCombine("Caches");

            public static readonly string MCBS_Caches_ColorMapping = MCBS_Caches.PathCombine("ColorMapping");

            public static readonly string MCBS_Applications = MCBS.PathCombine("Applications");

            public static readonly string MCBS_DllAppComponents = MCBS.PathCombine("DllAppComponents");

            public static readonly string MCBS_Logs = MCBS.PathCombine("Logs");

            public static readonly string MCBS_Logs_LatestLog = MCBS_Logs.PathCombine("Latest.log");

            public static readonly string MCBS_Minecraft = MCBS.PathCombine("Minecraft");

            public static readonly string MCBS_Minecraft_ResourcePacks = MCBS_Minecraft.PathCombine("ResourcePacks");

            public static readonly string MCBS_Minecraft_Vanilla = MCBS_Minecraft.PathCombine("Vanilla");

            public static readonly string MCBS_Minecraft_Vanilla_Version = MCBS_Minecraft_Vanilla.PathCombine("{0}");

            public static readonly string MCBS_Minecraft_Vanilla_Version_Languages = MCBS_Minecraft_Vanilla_Version.PathCombine("Languages");

            public static readonly string MCBS_Minecraft_Vanilla_Version_VersionJson = MCBS_Minecraft_Vanilla_Version.PathCombine("version.json");

            public static readonly string MCBS_Minecraft_Vanilla_Version_ClientCore = MCBS_Minecraft_Vanilla_Version.PathCombine("client.jar");

            public static readonly string MCBS_Minecraft_Vanilla_Version_IndexFile = MCBS_Minecraft_Vanilla_Version.PathCombine("index.json");

            public static readonly string MCBS_FFmpeg = MCBS.PathCombine("FFmpeg");

            public static readonly string MCBS_FFmpeg_Bin = MCBS_FFmpeg.PathCombine("Bin");

            public static readonly string MCBS_FFmpeg_Win64ZipFile = MCBS_FFmpeg.PathCombine("ffmpeg-n7.1-latest-win64-gpl-shared-7.1.zip");

            public static readonly string MCBS_FFmpeg_Win64IndexFile = MCBS_FFmpeg.PathCombine("ffmpeg-n7.1-latest-win64-gpl-shared-7.1.json");
        }

        [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
        private class CancelCreateAttribute : Attribute
        {

        }
    }
}
