﻿using QuanLib.Core.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.DirectoryManagers
{
    public class McbsDirectory : DirectoryManager
    {
        public McbsDirectory(string directory) : base(directory)
        {
            Applications = new(Combine("Applications"));
            Configs = new(Combine("Configs"));
            FFmpeg = new(Combine("FFmpeg"));
            MinecraftResources = new(Combine("MinecraftResources"));
            Saves = new(Combine("Saves"));
            SystemResources = new(Combine("SystemResources"));
        }

        public ApplicationsDirectory Applications { get; }

        public ConfigsDirectory Configs { get; }

        public FFmpegDirectory FFmpeg { get; }

        public MinecraftResourcesDirectory MinecraftResources { get; }

        public SavesDirectory Saves { get; }

        public SystemResourcesDirectory SystemResources { get; }
    }
}