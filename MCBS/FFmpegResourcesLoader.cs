using FFMediaToolkit;
using log4net.Core;
using Newtonsoft.Json;
using QuanLib.Core;
using QuanLib.IO;
using QuanLib.IO.Zip;
using QuanLib.Logging;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS
{
    public static class FFmpegResourcesLoader
    {
        private static readonly LogImpl LOGGER = LogManager.Instance.GetLogger();
        private const string FFMPEG_DOWMLOAD_URL = "https://github.com/BtbN/FFmpeg-Builds/releases/download/latest/ffmpeg-n7.1-latest-win64-gpl-shared-7.1.zip";
        private const string FFMPEG_BIN_DIR = "ffmpeg-n7.1-latest-win64-gpl-shared-7.1/bin/";

        public static void LoadAll()
        {
            try
            {
                BuildResourcesAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                LOGGER.Info("FFmpeg资源文件构建完成");

                FFmpegLoader.LoadFFmpeg();
                LOGGER.Info("FFmpeg资源文件加载完成");
            }
            catch (Exception ex)
            {
                LOGGER.Warn("FFmpeg资源文件加载失败，可能会影响到视频播放器组件的正常使用", ex);
            }
        }

        private static async Task BuildResourcesAsync()
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                ZipPack zipPack = await ReadOrDownloadFFmpegZipAsync(FFMPEG_DOWMLOAD_URL, McbsPathManager.MCBS_FFmpeg_Win64ZipFile.FullName);
                Dictionary<string, string> indexs = BuildFFmpegIndex(zipPack, McbsPathManager.MCBS_FFmpeg_Win64IndexFile.FullName);

                foreach (var index in indexs)
                {
                    string file = Path.Combine(McbsPathManager.MCBS_FFmpeg_Bin.FullName, index.Key);
                    if (!File.Exists(file) || HashUtil.GetHashString(file, HashType.SHA1) != index.Value)
                    {
                        using Stream stream = zipPack.GetFile(FFMPEG_BIN_DIR + index.Key).OpenStream();
                        using FileStream fileStream = new(file, FileMode.Create);
                        await stream.CopyToAsync(fileStream);
                        LOGGER.Info("已还原: " + file);
                    }
                }

                zipPack.Dispose();

                FFmpegLoader.FFmpegPath = McbsPathManager.MCBS_FFmpeg_Bin.FullName;
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

            return new(stream, ZipArchiveMode.Read, Encoding.UTF8, false);
        }

        private static Dictionary<string, string> BuildFFmpegIndex(ZipPack zipPack, string path)
        {
            ArgumentNullException.ThrowIfNull(zipPack, nameof(zipPack));
            ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));

            string[] files = zipPack.GetFilePaths(FFMPEG_BIN_DIR);
            Dictionary<string, string> indexs = new();
            foreach (var file in files)
            {
                ZipItem zipItem = zipPack.GetFile(file);
                using Stream stream = zipItem.OpenStream();
                string sha1 = HashUtil.GetHashString(stream, HashType.SHA1);
                indexs.Add(zipItem.Name, sha1);
            }

            string json = JsonConvert.SerializeObject(indexs);
            File.WriteAllText(path, json);

            return indexs;
        }
    }
}
