using QuanLib.Core.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Directorys
{
    public class McbsDirectory : DirectoryBase
    {
        public McbsDirectory(string directory) : base(directory)
        {
            ApplicationsDir = AddDirectory<ApplicationsDirectory>("Applications");
            CachesDir = AddDirectory<CachesDirectory>("Caches");
            ConfigsDir = AddDirectory<ConfigsDirectory>("Configs");
            DllAppComponentsDir = AddDirectory<DllAppComponentsDirectory>("DllAppComponents");
            LogsDir = AddDirectory<LogsDirectory>("Logs");
            FFmpegDir = AddDirectory<FFmpegDirectory>("FFmpeg");
            MinecraftDir = AddDirectory<MinecraftDirectory>("Minecraft");
        }

        public ApplicationsDirectory ApplicationsDir { get; }

        public CachesDirectory CachesDir { get; }

        public ConfigsDirectory ConfigsDir { get; }

        public DllAppComponentsDirectory DllAppComponentsDir { get; }

        public LogsDirectory LogsDir { get; }

        public FFmpegDirectory FFmpegDir { get; }

        public MinecraftDirectory MinecraftDir { get; }
    }
}
