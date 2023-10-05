using FFMediaToolkit;
using log4net.Core;
using MCBS.Logging;
using Newtonsoft.Json;
using QuanLib.Core.Extensions;
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
        private static readonly LogImpl LOGGER = LogUtil.GetLogger();
        private const string FFMPEG_BIN_DIR = "ffmpeg-master-latest-win64-gpl-shared/bin/";

        public static void LoadAll()
        {
            LOGGER.Info("开始加载FFmpeg资源文件");

            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                FFmpegLoader.FFmpegPath = SR.McbsDirectory.FFmpegDir.FullPath;
                Assembly assembly = Assembly.GetExecutingAssembly();

                using Stream indexsStream = assembly.GetManifestResourceStream(SR.SystemResourceNamespace.FFmpegIndexFile) ?? throw new InvalidOperationException();
                string indexsJson = indexsStream.ToUtf8Text();
                Dictionary<string, string> indexs = JsonConvert.DeserializeObject<Dictionary<string, string>>(indexsJson) ?? throw new InvalidOperationException();

                using Stream ffmpegStream = assembly.GetManifestResourceStream(SR.SystemResourceNamespace.FFmpegWin64BuildFile) ?? throw new InvalidOperationException();
                ZipPack ffmpegZipPack = new(ffmpegStream);

                foreach (var index in indexs)
                {
                    string file = Path.Combine(FFmpegLoader.FFmpegPath, index.Key);
                    if (!File.Exists(file) || HashUtil.GetHashString(file, HashType.SHA1) != index.Value)
                    {
                        using Stream stream = ffmpegZipPack[FFMPEG_BIN_DIR + index.Key].Open();
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
    }
}
