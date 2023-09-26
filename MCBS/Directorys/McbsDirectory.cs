﻿using QuanLib.Core.IO;
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
            LogsDir = AddDirectory<LogsDirectory>("Logs");
            FFmpegDir = AddDirectory<FFmpegDirectory>("FFmpeg");
            MinecraftResourcesDir = AddDirectory<MinecraftResourcesDirectory>("MinecraftResources");
            SavesDir = AddDirectory<SavesDirectory>("Saves");
        }

        public ApplicationsDirectory ApplicationsDir { get; }

        public ConfigsDirectory ConfigsDir { get; }

        public LogsDirectory LogsDir { get; }

        public FFmpegDirectory FFmpegDir { get; }

        public MinecraftResourcesDirectory MinecraftResourcesDir { get; }

        public SavesDirectory SavesDir { get; }
    }
}
