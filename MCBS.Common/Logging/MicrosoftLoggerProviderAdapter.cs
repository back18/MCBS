using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.Common.Logging
{
    public class MicrosoftLoggerProviderAdapter : ILoggerProvider
    {
        public MicrosoftLoggerProviderAdapter(QuanLib.Core.ILoggerProvider loggerProvider)
        {
            ArgumentNullException.ThrowIfNull(loggerProvider, nameof(loggerProvider));

            _loggerProvider = loggerProvider;
        }

        private readonly QuanLib.Core.ILoggerProvider _loggerProvider;

        public ILogger CreateLogger(string categoryName)
        {
            QuanLib.Core.ILogger logger = _loggerProvider.GetLogger(categoryName);
            return new MicrosoftLoggerAdapter(logger);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
