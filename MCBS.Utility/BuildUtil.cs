using Newtonsoft.Json;
using QuanLib.Core.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Utility
{
    public static class BuildUtil
    {
        public static void BuildFFmpegIndex(string ffmpegPath, string savePath)
        {
            ArgumentException.ThrowIfNullOrEmpty(ffmpegPath, nameof(ffmpegPath));
            ArgumentException.ThrowIfNullOrEmpty(savePath, nameof(savePath));

            ZipPack zipPack = new(ffmpegPath);
            ZipArchiveEntry[] entries = zipPack.GetFiles(Path.GetFileNameWithoutExtension(ffmpegPath) + "/bin/");
            Dictionary<string, string> indexs = new();
            foreach (var entry in entries)
            {
                using Stream stream = entry.Open();
                string sha1 = HashUtil.GetHashString(stream, HashType.SHA1);
                indexs.Add(entry.Name, sha1);
            }

            string json = JsonConvert.SerializeObject(indexs);
            File.WriteAllText(savePath, json);
        }

        public static void BuildTextureIndex(string texturesDir, string savePath)
        {
            ArgumentException.ThrowIfNullOrEmpty(texturesDir, nameof(texturesDir));
            ArgumentException.ThrowIfNullOrEmpty(savePath, nameof(savePath));

            string[] files = Directory.GetFiles(texturesDir, "*.png");
            Dictionary<string, string> indexs = new();
            foreach (string file in files)
            {
                string sha1 = HashUtil.GetHashString(file, HashType.SHA1);
                indexs.Add(Path.GetFileName(file), sha1);
            }

            string json = JsonConvert.SerializeObject(indexs);
            File.WriteAllText(savePath, json);
        }
    }
}
