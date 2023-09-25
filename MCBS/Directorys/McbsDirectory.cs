using QuanLib.Core.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Directorys
{
    public class McbsDirectory : DirectoryManager
    {
        public McbsDirectory(string directory) : base(directory)
        {
            Applications = new(Combine("Applications"));
            Configs = new(Combine("Configs"));
            Logs = new(Combine("Logs"));
            FFmpeg = new(Combine("FFmpeg"));
            MinecraftResources = new(Combine("MinecraftResources"));
            Saves = new(Combine("Saves"));
        }

        public ApplicationsDirectory Applications { get; }

        public ConfigsDirectory Configs { get; }

        public LogsDirectory Logs { get; }

        public FFmpegDirectory FFmpeg { get; }

        public MinecraftResourcesDirectory MinecraftResources { get; }

        public SavesDirectory Saves { get; }
    }
}
