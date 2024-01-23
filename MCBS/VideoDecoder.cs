using FFMediaToolkit.Decoding;
using FFMediaToolkit.Graphics;
using QuanLib.Core;
using QuanLib.Core.Events;
using QuanLib.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS
{
    public class VideoDecoder<TPixel> : UnmanagedRunnable where TPixel : unmanaged, IPixel<TPixel>
    {
        public VideoDecoder(VideoStream videoStream) : base(LogManager.Instance.Logbuilder)
        {
            ArgumentNullException.ThrowIfNull(videoStream, nameof(videoStream));

            MaxCacheFrames = 16;
            _videoStream = videoStream;
            _cacheFrames = new();

            JumpedToFrame += OnJumpedToFrame;
        }

        private readonly VideoStream _videoStream;

        private readonly ConcurrentQueue<VideoFrame<TPixel>> _cacheFrames;

        private TimeSpan? _jumpPosition;

        public int MaxCacheFrames { get; set; }

        public event EventHandler<VideoDecoder<TPixel>, TimeSpanEventArgs> JumpedToFrame;

        protected virtual void OnJumpedToFrame(VideoDecoder<TPixel> sender, TimeSpanEventArgs e)
        {
            while (_cacheFrames.TryDequeue(out var videoFrame))
                videoFrame.Dispose();
        }

        protected override void Run()
        {
            while (IsRunning)
            {
                if (_cacheFrames.Count >= MaxCacheFrames && _jumpPosition is null)
                {
                    Thread.Sleep(10);
                    continue;
                }

                if (!TryReadFrame(out var imageData))
                {
                    Thread.Sleep(10);
                    continue;
                }

                Image<TPixel> image = Image.LoadPixelData<TPixel>(imageData.Data, imageData.ImageSize.Width, imageData.ImageSize.Height);
                TimeSpan position = _videoStream.Position;
                _cacheFrames.Enqueue(new(image, position));
            }
        }

        protected override void DisposeUnmanaged()
        {
            Task task = WaitForStopAsync();
            IsRunning = false;
            task.Wait();

            while (_cacheFrames.TryDequeue(out var videoFrame))
                videoFrame.Dispose();
        }

        public bool TryGetNextFrame([MaybeNullWhen(false)] out VideoFrame<TPixel> videoFrame)
        {
            while (_cacheFrames.IsEmpty)
            {
                if (!IsRunning)
                {
                    videoFrame = null;
                    return false;
                }

                Thread.Yield();
            }

            return _cacheFrames.TryDequeue(out videoFrame);
        }

        public void JumpToFrame(TimeSpan position)
        {
            _jumpPosition = position;
        }

        private bool TryReadFrame(out ImageData imageData)
        {
            if (_jumpPosition is null)
                return _videoStream.TryGetNextFrame(out imageData);

            if (_videoStream.TryGetFrame(_jumpPosition.Value, out imageData))
            {
                JumpedToFrame.Invoke(this, new(_videoStream.Position));
                _jumpPosition = null;
                return true;
            }
            else
            {
                _jumpPosition = null;
                return false;
            }
        }
    }
}
