using FFMediaToolkit.Decoding;
using log4net.Core;
using MCBS.Events;
using MCBS.Logging;
using MCBS.State;
using NAudio.Wave;
using Newtonsoft.Json.Linq;
using QuanLib.Core;
using QuanLib.Core.Events;
using QuanLib.Minecraft;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS
{
    public class MediaFilePlayer<TPixel> : UnmanagedBase, ITickable where TPixel : unmanaged, IPixel<TPixel>
    {
        private static readonly LogImpl LOGGER = LogUtil.GetLogger();

        public MediaFilePlayer(string path, MediaOptions mediaOptions, bool enableAudio = true)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException($"“{nameof(path)}”不能为 null 或空。", nameof(path));

            MediaFile = MediaFile.Open(path, mediaOptions);
            VideoDecoder = new(MediaFile.Video);
            EnableAudio = enableAudio;
            if (EnableAudio)
            {
                try
                {
                    MediaFoundationReader = new(path);
                    WaveOutEvent = new();
                    WaveOutEvent.Init(MediaFoundationReader);
                    WaveOutEvent.Volume = 0.25f;
                }
                catch (Exception ex)
                {
                    WaveOutEvent?.Dispose();
                    MediaFoundationReader?.Dispose();
                    WaveOutEvent = null;
                    MediaFoundationReader = null;
                    EnableAudio = false;
                    LOGGER.Error("无法使用NAudio库播放音频，音频已禁用", ex);
                }
            }

            StateManager = new(MediaFilePlayerState.Unstarted, new StateContext<MediaFilePlayerState>[]
            {
                new(MediaFilePlayerState.Unstarted, Array.Empty<MediaFilePlayerState>(), HandleUnstartedState),
                new(MediaFilePlayerState.Playing, new MediaFilePlayerState[] { MediaFilePlayerState.Unstarted, MediaFilePlayerState.Pause }, HandlePlayingState, OnPlayingState),
                new(MediaFilePlayerState.Pause, new MediaFilePlayerState[] { MediaFilePlayerState.Playing }, HandlePauseState),
                new(MediaFilePlayerState.Ended, new MediaFilePlayerState[] { MediaFilePlayerState.Pause }, HandleEndedState),
            });

            _start = TimeSpan.Zero;
            _stopwatch = new();

            VideoFrameChanged += OnVideoFrameChanged;
            Played += OnPlayed;
            Paused += OnPaused;
            VideoDecoder.JumpedToFrame += VideoDecoder_JumpedToFrame;
        }

        private TimeSpan _start;

        private readonly Stopwatch _stopwatch;

        public bool EnableAudio { get; }

        public MediaFile MediaFile { get; }

        public VideoDecoder<TPixel> VideoDecoder { get; }

        public MediaFoundationReader? MediaFoundationReader { get; }

        public WaveOutEvent? WaveOutEvent { get; }

        public VideoFrame<TPixel>? CurrentVideoFrame { get; private set; }

        public float Volume
        {
            get => WaveOutEvent?.Volume ?? 0;
            set
            {
                if (WaveOutEvent is null)
                    return;

                if (value < 0)
                    value = 0;
                else if (value > 1)
                    value = 1;

                WaveOutEvent.Volume = value;
            }
        }

        public TimeSpan CurrentPosition
        {
            get => CurrentVideoFrame?.Position ?? TimeSpan.Zero;
            set => JumpToFrame(value);
        }

        public TimeSpan TotalTime => MediaFile.Info.Duration;

        public StateManager<MediaFilePlayerState> StateManager { get; }

        public MediaFilePlayerState PlayerState => StateManager.CurrentState;

        public event EventHandler<MediaFilePlayer<TPixel>, EventArgs> Played;

        public event EventHandler<MediaFilePlayer<TPixel>, EventArgs> Paused;

        public event EventHandler<MediaFilePlayer<TPixel>, VideoFrameChangedEventArgs<TPixel>> VideoFrameChanged;

        protected virtual void OnPlayed(MediaFilePlayer<TPixel> sender, EventArgs e) { }

        protected virtual void OnPaused(MediaFilePlayer<TPixel> sender, EventArgs e) { }

        protected virtual void OnVideoFrameChanged(MediaFilePlayer<TPixel> sender, VideoFrameChangedEventArgs<TPixel> e)
        {
            e.OldVideoFrame?.Dispose();
        }

        private void VideoDecoder_JumpedToFrame(VideoDecoder<TPixel> sender, TimeSpanEventArgs e)
        {
            _start = e.TimeSpan;
            if (EnableAudio)
            {
                if (MediaFoundationReader is not null)
                    MediaFoundationReader.CurrentTime = _start;
                WaveOutEvent?.Play();
            }

            _stopwatch.Restart();
        }

        protected virtual bool HandleUnstartedState(MediaFilePlayerState current, MediaFilePlayerState next)
        {
            return false;
        }

        protected virtual bool HandlePlayingState(MediaFilePlayerState current, MediaFilePlayerState next)
        {
            switch (current)
            {
                case MediaFilePlayerState.Unstarted:
                    VideoDecoder.Start("VideoDecoder Thread");
                    if (EnableAudio)
                        WaveOutEvent?.Play();
                    _stopwatch.Start();
                    Played.Invoke(this, EventArgs.Empty);
                    return true;
                case MediaFilePlayerState.Pause:
                    if (EnableAudio)
                        WaveOutEvent?.Play();
                    _stopwatch.Start();
                    Played.Invoke(this, EventArgs.Empty);
                    return true;
                default:
                    return false;
            }
        }

        protected virtual bool HandlePauseState(MediaFilePlayerState current, MediaFilePlayerState next)
        {
            if (EnableAudio)
                WaveOutEvent?.Stop();
            _stopwatch.Stop();
            Paused.Invoke(this, EventArgs.Empty);
            return true;
        }

        protected virtual bool HandleEndedState(MediaFilePlayerState current, MediaFilePlayerState next)
        {
            Dispose();
            return true;
        }

        protected virtual void OnPlayingState()
        {
            VideoFrame<TPixel>? videoFrame;
            while (true)
            {
                if (!VideoDecoder.TryGetNextFrame(out videoFrame))
                {
                    Pause();
                    break;
                }

                double next = videoFrame.Position.TotalMilliseconds + Math.Round(1000 / MediaFile.Video.Info.AvgFrameRate);
                double now = (_start + _stopwatch.Elapsed).TotalMilliseconds;
                if (Math.Abs(now - videoFrame.Position.TotalMilliseconds) < Math.Abs(now - next))
                {
                    break;
                }
                else
                {
                    videoFrame.Dispose();
                }
            }

            VideoFrame<TPixel>? temp = CurrentVideoFrame;
            CurrentVideoFrame = videoFrame;
            VideoFrameChanged.Invoke(this, new(temp, CurrentVideoFrame));
        }

        public void OnTick()
        {
            StateManager.HandleAllState();
        }

        protected override void DisposeUnmanaged()
        {
            VideoDecoder.Stop();
            MediaFile.Dispose();
            WaveOutEvent?.Dispose();
            MediaFoundationReader?.Dispose();
            CurrentVideoFrame?.Dispose();
        }

        public void JumpToFrame(TimeSpan position)
        {
            VideoDecoder.JumpToFrame(position);
        }

        public void Play()
        {
            StateManager.AddNextState(MediaFilePlayerState.Playing);
        }

        public void Pause()
        {
            StateManager.AddNextState(MediaFilePlayerState.Pause);
        }
    }
}
