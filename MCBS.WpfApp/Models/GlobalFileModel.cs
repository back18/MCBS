using iNKORE.UI.WPF.Modern.Common.IconKeys;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.WpfApp.Models
{
    public record GlobalFileModel(string FileName, long Size, GlobalFileStatus Status, FontIconData IconData)
    {
        public GlobalFileModel(string FileName, long Size, GlobalFileStatus Status)
            : this(FileName, Size, Status, SegoeFluentIcons.Document) { }
    }
}
