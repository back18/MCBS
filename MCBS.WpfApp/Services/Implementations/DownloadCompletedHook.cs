using Downloader;
using MCBS.WpfApp.Events;
using Microsoft.Extensions.Logging;
using QuanLib.Minecraft.Downloading;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MCBS.WpfApp.Services.Implementations
{
    public abstract class DownloadCompletedHook<T>(ILogger? logger) : IDownloadCompletedHook<T>
    {
        protected ILogger? _logger = logger;

        protected readonly SemaphoreSlim _semaphore = new(1);
        protected IDownloadViewModel? _downloadViewModel;
        protected bool _isReady;
        protected bool _isDisposed;

        public virtual MemoryStream? CloneStream { get; protected set; }

        public virtual bool IsBinding => _downloadViewModel is not null;

        public virtual bool IsReady => _isReady;

        public abstract Task<T> LoadContentAsync();

        public virtual void Binding(IDownloadViewModel downloadViewModel)
        {
            _semaphore.Wait();

            try
            {
                ObjectDisposedException.ThrowIf(_isDisposed, this);
                if (_downloadViewModel is not null)
                    throw new InvalidOperationException("Already bound to a download view model.");

                _downloadViewModel = downloadViewModel;
                _downloadViewModel.DownloadCompleted += OnDownloadCompleted;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public virtual void Unbinding()
        {
            _downloadViewModel?.DownloadCompleted -= OnDownloadCompleted;
            _downloadViewModel = null;
        }

        protected virtual async void OnDownloadCompleted(object? sender, FileDownloadCompletedEventArgs e)
        {
            if (_isDisposed)
            {
                _logger?.LogWarning("事件钩子已被释放，无法处理事件");
                return; 
            }

            if (e.Status != DownloadStatus.Completed)
            {
                _logger?.LogWarning("下载未成功完成，事件钩子无法拷贝流，状态: {Status}", e.Status);
                return;
            }

            Stream? resultStream = e.ResultStream;
            if (resultStream is null || resultStream == Stream.Null)
            {
                _logger?.LogWarning("下载已完成，但下载结果流为空，事件钩子无法拷贝流");
                return;
            }

            if (resultStream.CanSeek && resultStream.Position != 0)
                resultStream.Seek(0, SeekOrigin.Begin);

            try
            {
                _isReady = false;
                int capacity = resultStream.CanSeek ? (int)resultStream.Length : 200_000;
                CloneStream?.Dispose();
                CloneStream = new MemoryStream(capacity);
                await resultStream.CopyToAsync(CloneStream);
                _isReady = true;

                if (_logger?.IsEnabled(LogLevel.Information) == true)
                    _logger.LogInformation("事件钩子成功拷贝下载结果流，长度: {Length}", CloneStream.Length);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "事件钩子拷贝下载结果流时发生错误");
            }
            finally
            {
                if (resultStream.CanSeek && resultStream.Position != 0)
                    resultStream.Seek(0, SeekOrigin.Begin);
            }
        }

        public virtual void Dispose()
        {
            Unbinding();

            _isDisposed = true;
            _semaphore.Dispose();
            CloneStream?.Dispose();
            CloneStream = null;
            GC.SuppressFinalize(this);
        }
    }
}
