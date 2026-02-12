using MCBS.Services;
using QuanLib.Consoles;
using QuanLib.Core;
using QuanLib.IO.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace MCBS.ConsoleTerminal.Services
{
    public class CharacterWidthMappingLoader
    {
        public string DEFAULT_FILE_NAME = "CharacterWidthMapping.bin";

        public CharacterWidthMappingLoader(ILoggerProvider loggerProvider, ICachePathProvider cachePathProvider, string? fileName = null)
        {
            ArgumentNullException.ThrowIfNull(loggerProvider, nameof(loggerProvider));
            ArgumentNullException.ThrowIfNull(cachePathProvider, nameof(cachePathProvider));

            _logger = loggerProvider.GetLogger();
            _cachePathProvider = cachePathProvider;
            _fileName = string.IsNullOrEmpty(fileName) ? DEFAULT_FILE_NAME : fileName;
        }

        private readonly ILogger _logger;
        private readonly ICachePathProvider _cachePathProvider;
        private readonly string _fileName;

        public async Task<CharacterWidthMapping> LoadAsync()
        {
            FileInfo fileInfo = _cachePathProvider.Cache.CombineFile(_fileName);
            string filePath = fileInfo.FullName;
            byte[] bytes;

            if (ValidateFile(fileInfo))
            {
                try
                {
                    bytes = await File.ReadAllBytesAsync(filePath).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.Error("控制台字符宽度映射表缓存读取失败", ex);
                    bytes = await RunCollector().ConfigureAwait(false);
                    await TrySaveFileAsync(filePath, bytes).ConfigureAwait(false);
                }
            }
            else
            {
                bytes = await RunCollector().ConfigureAwait(false);
                await TrySaveFileAsync(filePath, bytes).ConfigureAwait(false);
            }

            return CharacterWidthMapping.LoadInstance(new(bytes));
        }

        private async Task<byte[]> RunCollector()
        {
            _logger.Info("开始采集控制台全部字符宽度 0x0000 -> 0xffff");
            await Task.Delay(100).ConfigureAwait(false);

            Stopwatch stopwatch = Stopwatch.StartNew();
            byte[] bytes = await Task.Run(CharacterWidthCollector.Run).ConfigureAwait(false);
            stopwatch.Stop();

            await Task.Delay(100).ConfigureAwait(false);
            _logger.Info($"完成！耗时 {(int)stopwatch.Elapsed.TotalSeconds} 秒");
            return bytes;
        }

        private async Task TrySaveFileAsync(string path, byte[] bytes)
        {
            try
            {
                if (File.Exists(path))
                    File.Delete(path);

                await File.WriteAllBytesAsync(path, bytes).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.Error("控制台字符宽度映射表缓存保存失败", ex);
            }
        }

        private static bool ValidateFile(FileInfo fileInfo)
        {
            if (!fileInfo.Exists)
                return false;

            if (fileInfo.Length != 65536)
                return false;

            return true;
        }
    }
}
