using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.Common.Services
{
    public interface IFileFactory
    {
        public Stream CreateStream(string? key = null);
    }
}
