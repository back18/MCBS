using MCBS.Drawing;
using QuanLib.Core;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace MCBS.Common
{
    public class ColorMappingWorker : INotifyPropertyChanged
    {
        public ColorMappingWorker(
            IColorFinder colorFinder,
            IColorToIndexConverter colorToIndexConverter,
            Rgba32[] buffer,
            int startIndex,
            int count,
            IProgress<BuildProgress>? progress = null)
        {
            ArgumentNullException.ThrowIfNull(colorFinder, nameof(colorFinder));
            ArgumentNullException.ThrowIfNull(colorToIndexConverter, nameof(colorToIndexConverter));
            ArgumentNullException.ThrowIfNull(buffer, nameof(buffer));
            ThrowHelper.ArgumentOutOfRange(0, buffer.Length - 1, startIndex, nameof(buffer));
            ThrowHelper.ArgumentOutOfRange(0, buffer.Length - startIndex, count, nameof(count));

            _colorFinder = colorFinder;
            _colorToIndexConverter = colorToIndexConverter;
            _buffer = buffer;
            _startIndex = startIndex;
            _count = count;
            _progress = progress;
            _tcs = new();

            MinProgressInterval = 1000;
            ThreadPriority = ThreadPriority.Highest;
        }

        private readonly Lock _lock = new();
        private readonly IColorFinder _colorFinder;
        private readonly IColorToIndexConverter _colorToIndexConverter;
        private readonly Rgba32[] _buffer;
        private readonly int _startIndex;
        private readonly int _count;
        private int _completed;
        private readonly IProgress<BuildProgress>? _progress;
        private readonly TaskCompletionSource _tcs;
        private bool _running;
        private bool _canceled;

        public int MinProgressInterval
        {
            get => field;
            set
            {
                ThrowHelper.ArgumentOutOfMin(1, value, nameof(MinProgressInterval));
                if (value != field)
                    OnPropertyChanged(ref field, value);
            }
        }

        public ThreadPriority ThreadPriority
        {
            get => field;
            set
            {
                if (value != field)
                    OnPropertyChanged(ref field, value);
            }
        }

        public int TotalCount => _count;

        public int CompletedCount => _completed;

        public double ProgressPercentage => TotalCount == 0 ? 0 : ((double)CompletedCount * 100) / TotalCount;

        public event PropertyChangedEventHandler? PropertyChanged;

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            lock (_lock)
            {
                if (_running)
                    throw new InvalidOperationException("任务已运行，无法重复启动");
                _running = true;
            }

            Thread thread = await Task.Run(CreateThread, cancellationToken).ConfigureAwait(false);
            thread.Start();

            Task task = _tcs.Task;
            TimeSpan interval = TimeSpan.FromMilliseconds(Math.Max(1, MinProgressInterval));

            while (true)
            {
                ReportProgress();

                try
                {
                    await task.WaitAsync(interval, cancellationToken).ConfigureAwait(false);
                    break;
                }
                catch (TimeoutException)
                {
                    continue;
                }
                catch (OperationCanceledException)
                {
                    _canceled = true;
                    throw;
                }
            }

            ReportProgress();

            cancellationToken.ThrowIfCancellationRequested();
            if (CompletedCount < TotalCount)
                throw new InvalidDataException($"工作区未能完全填充 ({CompletedCount} / {TotalCount})", task.Exception);
        }

        private Thread CreateThread()
        {
            return new Thread(Build)
            {
                Priority = ThreadPriority,
                IsBackground = true
            };
        }

        private void Build()
        {
            try
            {
                int endIndex = _startIndex + _count;
                for (int i = _startIndex; i < endIndex && !_canceled; i++)
                {
                    _buffer[i] = _colorFinder.Find(_colorToIndexConverter.ToColor(i));
                    _completed++;
                }

                if (_canceled)
                    _tcs.SetCanceled();
                else
                    _tcs.SetResult();
            }
            catch (Exception ex)
            {
                _tcs.SetException(ex);
            }
        }

        private void ReportProgress()
        {
            _progress?.Report(new BuildProgress(_count, _completed));
            OnPropertyChanged(nameof(CompletedCount));
            OnPropertyChanged(nameof(ProgressPercentage));
        }

        protected virtual void OnPropertyChanged<T>(ref T field, T newValue, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, newValue))
                return;

            field = newValue;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnPropertyChanged(string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
