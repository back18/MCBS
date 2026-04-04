using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MCBS.WpfApp.Models
{
    public record class AssetFileModel(FileInfo FileInfo, string AssetPath, string AssetName);
}
