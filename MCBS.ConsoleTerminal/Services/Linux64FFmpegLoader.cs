using FFMediaToolkit;
using MCBS.Common.Services;
using QuanLib.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.ConsoleTerminal.Services
{
    public class Linux64FFmpegLoader : IFFmpegLoader
    {
        public Linux64FFmpegLoader(ILoggerProvider loggerProvider)
        {
            ArgumentNullException.ThrowIfNull(loggerProvider, nameof(loggerProvider));

            _logger = loggerProvider.GetLogger();
        }

        private readonly ILogger _logger;

        public Task LoadAsync()
        {
            try
            {
                FFmpegLoader.FFmpegPath = "/usr/lib/";
                FFmpegLoader.LoadFFmpeg();
                _logger.Info("FFmpeg资源文件加载完成");
            }
            catch (Exception ex)
            {
                _logger.Error("FFmpeg资源文件加载失败，视频模块不可用", ex);
            }

            return Task.CompletedTask;
        }
    }
}
