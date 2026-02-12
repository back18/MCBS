using MCBS.Services;
using QuanLib.Core;
using QuanLib.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.Common.Services
{
    public class Log4NetLoader : ILoggingLoader
    {
        public Log4NetLoader(IConfigPathProvider configPathProvider, ILogPathProvider logPathProvider)
        {
            ArgumentNullException.ThrowIfNull(configPathProvider, nameof(configPathProvider));
            ArgumentNullException.ThrowIfNull(logPathProvider, nameof(logPathProvider));

            _configPathProvider = configPathProvider;
            _logPathProvider = logPathProvider;
        }

        private readonly IConfigPathProvider _configPathProvider;
        private readonly ILogPathProvider _logPathProvider;

        public ILoggerProvider Load()
        {
            using FileStream fileStream = _configPathProvider.Log4NetConfig.OpenRead();
            Log4NetProvider provider = new(_logPathProvider.LatestLog.FullName, fileStream, true);
            Log4NetManager.LoadInstance(new(provider));
            return provider;
        }
    }
}
