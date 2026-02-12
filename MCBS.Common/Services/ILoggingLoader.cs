using QuanLib.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.Common.Services
{
    public interface ILoggingLoader
    {
        public ILoggerProvider Load();
    }
}
