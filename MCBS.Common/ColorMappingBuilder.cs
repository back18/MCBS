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
    public class ColorMappingBuilder : INotifyPropertyChanged
    {
        private const int BUFFER_SIZE = 256 * 256 * 256;

        public ColorMappingBuilder(IColorFinder colorFinder, IColorToIndexConverter colorToIndexConverter, IProgress<BuildProgress>? progress = null)
        {
            ArgumentNullException.ThrowIfNull(colorFinder, nameof(colorFinder));
            ArgumentNullException.ThrowIfNull(colorToIndexConverter, nameof(colorToIndexConverter));

            _colorFinder = colorFinder;
            _colorToIndexConverter = colorToIndexConverter;
            _progress = progress;

            ChunkCount = Environment.ProcessorCount - 1;
            MinProgressInterval = 1000;
            ThreadPriority = ThreadPriority.Highest;
        }

        private readonly Lock _lock = new();
        private readonly IColorFinder _colorFinder;
        private readonly IColorToIndexConverter _colorToIndexConverter;
        private readonly IProgress<BuildProgress>? _progress;
        private ColorMappingWorker[]? _workers;
        private bool _running;

        public int TotalCount => BUFFER_SIZE;

        public int CompletedCount => _workers is null ? 0 : _workers.Sum(s => s.CompletedCount);

        public double ProgressPercentage => TotalCount == 0 ? 0 : ((double)CompletedCount * 100) / TotalCount;

        public int ChunkCount
        {
            get => field;
            set
            {
                ThrowHelper.ArgumentOutOfRange(1, Environment.ProcessorCount, value, nameof(ChunkCount));
                if (value != field)
                    OnPropertyChanged(ref field, value);
            }
        }

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

        public event PropertyChangedEventHandler? PropertyChanged;

        public async Task<Rgba32[]> BuildAsync(CancellationToken cancellationToken = default)
        {
            lock (_lock)
            {
                if (_running)
                    throw new InvalidOperationException("任务已运行，无法重复启动");
                _running = true;
            }

            Rgba32[] buffer = await Task.Run(CreateBuffer).ConfigureAwait(false);
            int chunkCount = Math.Max(1, ChunkCount);
            int countPerChunk = (int)Math.Ceiling((double)buffer.Length / chunkCount);
            ColorMappingWorker[] workers = new ColorMappingWorker[chunkCount];
            Task[] tasks = new Task[chunkCount];

            for (int i = 0; i < chunkCount; i++)
            {
                int startIndex = countPerChunk * i;
                int count = Math.Clamp(countPerChunk, 0, buffer.Length - startIndex);
                ColorMappingWorker worker = new(_colorFinder, _colorToIndexConverter, buffer, startIndex, count);

                workers[i] = worker;
                tasks[i] = worker.StartAsync(cancellationToken);
            }

            _workers = workers;
            Task mergeTask = Task.WhenAll(tasks);
            TimeSpan interval = TimeSpan.FromMilliseconds(Math.Max(1, MinProgressInterval));

            while (true)
            {
                ReportProgress();

                try
                {
                    await mergeTask.WaitAsync(interval, cancellationToken).ConfigureAwait(false);
                    break;
                }
                catch (TimeoutException)
                {
                    continue;
                }
            }

            ReportProgress();
            await Task.Yield();

            cancellationToken.ThrowIfCancellationRequested();
            if (CompletedCount < TotalCount)
                throw new InvalidDataException($"缓冲区未能完全填充 ({CompletedCount} / {TotalCount})", mergeTask.Exception);

            return buffer;
        }

        private Rgba32[] CreateBuffer()
        {
            return new Rgba32[BUFFER_SIZE];
        }

        private void ReportProgress()
        {
            _progress?.Report(new BuildProgress(TotalCount, CompletedCount));
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
