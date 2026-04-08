using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.WpfApp.Models
{
    public record GlobalFileModel(string FileName, long Size, GlobalFileStatus Status);
}
