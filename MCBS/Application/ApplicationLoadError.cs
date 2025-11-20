using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Application
{
    public enum ApplicationLoadError
    {
        UnknownError = 0,

        PathTooLong = 101,

        FileNotFound = 102,

        FileLoadFailed = 103,

        FileSecurityError = 104,

        AssemblyLoadFailed = 201,

        ManifestNotFound = 301,

        ManifestLoadFailed = 302,

        ManifestFormatError = 303,

        ClassLoadFailed = 304
    }
}
