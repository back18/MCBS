using QuanLib.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Namespaces
{
    public class SystemResourceNamespace : NamespaceBase
    {
        public SystemResourceNamespace(string @namespace) : base(@namespace)
        {
            ConfigsNamespace = AddNamespace<ConfigsNamespace>("Configs");
            CursorsNamespace = AddNamespace<CursorsNamespace>("Cursors");
            CursorIndexFile = Combine("CursorIndex.json");
            DefaultFontFile = Combine("DefaultFont.bdf");
            DefaultIconFile = Combine("DefaultIcon.png");
            FFmpegIndexFile = Combine("FFmpegIndex.json");
            FFmpegWin64BuildFile = Combine("ffmpeg-master-latest-win64-gpl-shared.zip");
        }

        public ConfigsNamespace ConfigsNamespace { get; }

        public CursorsNamespace CursorsNamespace { get; }

        public string CursorIndexFile { get; }

        public string DefaultFontFile { get; }

        public string DefaultIconFile { get; }

        public string FFmpegIndexFile { get; }

        public string FFmpegWin64BuildFile { get; }
    }
}
