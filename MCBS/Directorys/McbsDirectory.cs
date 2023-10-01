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
            ApplicationsDir = AddDirectory<ApplicationsDirectory>("Applications");
            ConfigsDir = AddDirectory<ConfigsDirectory>("Configs");
            DllAppComponentsDir = AddDirectory<DllAppComponentsDirectory>("DllAppComponents");
            LogsDir = AddDirectory<LogsDirectory>("Logs");
            FFmpegDir = AddDirectory<FFmpegDirectory>("FFmpeg");
            MinecraftResourcesDir = AddDirectory<MinecraftResourcesDirectory>("MinecraftResources");
        }

        public ApplicationsDirectory ApplicationsDir { get; }

        public ConfigsDirectory ConfigsDir { get; }

        public DllAppComponentsDirectory DllAppComponentsDir { get; }

        public LogsDirectory LogsDir { get; }

        public FFmpegDirectory FFmpegDir { get; }

        public MinecraftResourcesDirectory MinecraftResourcesDir { get; }
    }
}
