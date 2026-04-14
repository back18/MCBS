using QuanLib.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.Common
{
    public record DownloadAsset(string Url, HashType HashType, string HashValue, int FileSize, string FilePath);
}
