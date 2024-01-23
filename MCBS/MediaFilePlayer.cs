using FFMediaToolkit.Decoding;
using log4net.Core;
using MCBS.Events;
using NAudio.Wave;
using Newtonsoft.Json.Linq;
using QuanLib.Core;
using QuanLib.Core.Events;
using QuanLib.Logging;
using QuanLib.Minecraft;
using QuanLib.TickLoop;
using QuanLib.TickLoop.StateMachine;
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
    public class MediaFilePlayer<TPixel> : UnmanagedBase, ITickUpdatable where TPixel : unmanaged, IPixel<TPixel>
    {
        private static readonly LogImpl LOGGER = LogManager.Instance.GetLogger();

        public MediaFilePlayer(string path, MediaOptions mediaOptions, bool enableAudio = true)
        {
            ArgumentException.ThrowIfNullOrEmpty(path, nameof(path));

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

            StateMachine = new(MediaFilePlayerState.Unstarted, new StateContext<MediaFilePlayerState>[]
            {
                new(MediaFilePlayerState.Unstarted, Array.Empty<MediaFilePlayerState>(), GotoUnstartedState),
                new(MediaFilePlayerState.Playing, new MediaFilePlayerState[] { MediaFilePlayerState.Unstarted, MediaFilePlayerState.Pause }, GotoPlayingState, PlayingStateUpdate),
                new(MediaFilePlayerState.Pause, new MediaFilePlayerState[] { MediaFilePlayerState.Playing }, GotoPauseState),
                new(MediaFilePlayerState.Ended, new MediaFilePlayerState[] { MediaFilePlayerState.Pause }, GotoEndedState),
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

        public TickStateMachine<MediaFilePlayerState> StateMachine { get; }

        public MediaFilePlayerState PlayerState => StateMachine.CurrentState;

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

        protected virtual bool GotoUnstartedState(MediaFilePlayerState sourceState, MediaFilePlayerState targetState)
        {
            return false;
        }

        protected virtual bool GotoPlayingState(MediaFilePlayerState sourceState, MediaFilePlayerState targetState)
        {
            switch (sourceState)
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

        protected virtual bool GotoPauseState(MediaFilePlayerState sourceState, MediaFilePlayerState targetState)
        {
            if (EnableAudio)
                WaveOutEvent?.Stop();
            _stopwatch.Stop();
            Paused.Invoke(this, EventArgs.Empty);
            return true;
        }

        protected virtual bool GotoEndedState(MediaFilePlayerState sourceState, MediaFilePlayerState targetState)
        {
            Dispose();
            return true;
        }

        protected virtual void PlayingStateUpdate(int tick)
        {
            VideoFrame<TPixel>? videoFrame;
            while (true)
            {
                if (!VideoDecoder.TryGetNextFrame(out videoFrame))
                {
                    Pause();
                    break;
                }

                double targetState = videoFrame.Position.TotalMilliseconds + Math.Round(1000 / MediaFile.Video.Info.AvgFrameRate);
                double now = (_start + _stopwatch.Elapsed).TotalMilliseconds;
                if (Math.Abs(now - videoFrame.Position.TotalMilliseconds) < Math.Abs(now - targetState))
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

        public void OnTickUpdate(int tick)
        {
            StateMachine.OnTickUpdate(tick);
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
            StateMachine.Submit(MediaFilePlayerState.Playing);
        }

        public void Pause()
        {
            StateMachine.Submit(MediaFilePlayerState.Pause);
        }
    }
}
