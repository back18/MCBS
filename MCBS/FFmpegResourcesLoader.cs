using FFMediaToolkit;
using log4net.Core;
using MCBS.Directorys;
using MCBS.Logging;
using Newtonsoft.Json;
using QuanLib.Core.Extensions;
using QuanLib.Core.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MCBS
{
    public static class FFmpegResourcesLoader
    {
        private static readonly LogImpl LOGGER = LogUtil.GetLogger();
        private const string FFMPEG_ZIP_URL = "https://github.com/BtbN/FFmpeg-Builds/releases/download/latest/ffmpeg-master-latest-win64-gpl-shared.zip";
        private const string FFMPEG_BIN_DIR = "ffmpeg-master-latest-win64-gpl-shared/bin/";

        public static void LoadAll()
        {
            LOGGER.Info("开始加载FFmpeg资源文件");

            BuildResourcesAsync().Wait();

            try
            {
                FFmpegLoader.LoadFFmpeg();
                LOGGER.Info("完成");
            }
            catch (Exception ex)
            {
                LOGGER.Warn("FFmpeg加载失败，可能会影响到视频解码器/播放器组件的正常使用", ex);
            }
        }

        private static async Task BuildResourcesAsync()
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                FFmpegDirectory ffmpegDir = SR.McbsDirectory.FFmpegDir;
                FFmpegLoader.FFmpegPath = ffmpegDir.BinDir.FullPath;

                ZipPack zipPack = await ReadOrDownloadFFmpegZipAsync(FFMPEG_ZIP_URL, ffmpegDir.FFmpegWin64ZipFile);
                Dictionary<string, string> indexs = BuildFFmpegIndex(zipPack, ffmpegDir.FFmpegWin64IndexFile);

                foreach (var index in indexs)
                {
                    string file = Path.Combine(FFmpegLoader.FFmpegPath, index.Key);
                    if (!File.Exists(file) || HashUtil.GetHashString(file, HashType.SHA1) != index.Value)
                    {
                        using Stream stream = zipPack[FFMPEG_BIN_DIR + index.Key].Open();
                        using FileStream fileStream = new(file, FileMode.Create);
                        stream.CopyTo(fileStream);
                        LOGGER.Info("已还原: " + file);
                    }
                }

                zipPack.Dispose();
            }
            else
            {
                FFmpegLoader.FFmpegPath = "/usr/lib/";
            }
        }

        private static async Task<ZipPack> ReadOrDownloadFFmpegZipAsync(string url, string path)
        {
            ArgumentException.ThrowIfNullOrEmpty(url, nameof(url));
            ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));

            Stream stream;
            if (File.Exists(path))
                stream = File.OpenRead(path);
            else
                stream = await DownloadHelper.DownloadAsync(url, path);

            return new(stream);
        }

        private static Dictionary<string, string> BuildFFmpegIndex(ZipPack zipPack, string path)
        {
            ArgumentNullException.ThrowIfNull(zipPack, nameof(zipPack));
            ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));

            ZipArchiveEntry[] entries = zipPack.GetFiles(FFMPEG_BIN_DIR);
            Dictionary<string, string> indexs = new();
            foreach (var entry in entries)
            {
                using Stream stream = entry.Open();
                string sha1 = HashUtil.GetHashString(stream, HashType.SHA1);
                indexs.Add(entry.Name, sha1);
            }

            string json = JsonConvert.SerializeObject(indexs);
            File.WriteAllText(path, json);

            return indexs;
        }
    }
}
