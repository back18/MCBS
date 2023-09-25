using FFMediaToolkit;
using log4net.Core;
using MCBS.Logging;
using Newtonsoft.Json;
using QuanLib.Core.Extension;
using QuanLib.Core.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MCBS
{
    public static class FFmpegResourcesLoader
    {
        private static readonly LogImpl LOGGER = LogUtil.MainLogger;
        private const string FFMPEG_INDEX_NAME = SR.SYSTEM_RESOURCE_NAMESPACE + ".FFmpegIndex.json";
        private const string FFMPEG_NAME = SR.SYSTEM_RESOURCE_NAMESPACE + ".ffmpeg-master-latest-win64-gpl-shared.zip";
        private const string FFMPEG_BIN_DIR = "ffmpeg-master-latest-win64-gpl-shared/bin/";

        public static void LoadAll()
        {
            LOGGER.Info("开始加载FFmpeg资源文件");

            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                MCOS.MainDirectory.FFmpeg.CreateIfNotExists();
                FFmpegLoader.FFmpegPath = MCOS.MainDirectory.FFmpeg.FullPath;
                Assembly assembly = Assembly.GetExecutingAssembly();

                using Stream indexsStream = assembly.GetManifestResourceStream(FFMPEG_INDEX_NAME) ?? throw new InvalidOperationException();
                string indexsJson = indexsStream.ToUtf8Text();
                Dictionary<string, string> indexs = JsonConvert.DeserializeObject<Dictionary<string, string>>(indexsJson) ?? throw new InvalidOperationException();

                using Stream ffmpegStream = assembly.GetManifestResourceStream(FFMPEG_NAME) ?? throw new InvalidOperationException();
                ZipPack zipPack = new(ffmpegStream);

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
            }
            else
            {
                FFmpegLoader.FFmpegPath = "/usr/lib/";
            }

            LOGGER.Info("完成");
        }
    }
}
