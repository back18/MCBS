using iNKORE.UI.WPF.Modern.Common.IconKeys;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MCBS.WpfApp.Models
{
    public record FileModel(FileInfo? FileInfo, string LocalizationKey, FontIconData FileIcon);
}
