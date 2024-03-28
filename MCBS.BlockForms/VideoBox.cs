using FFMediaToolkit.Decoding;
using FFMediaToolkit.Graphics;
using MCBS.BlockForms.Utility;
using MCBS.Events;
using MCBS.Rendering;
using MCBS.Rendering.Extensions;
using Microsoft.VisualBasic;
using NAudio.Midi;
using NAudio.Wave;
using QuanLib.Core;
using QuanLib.TickLoop.VideoPlayer;
using QuanLib.TickLoop.VideoPlayer.Events;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.BlockForms
{
    public class VideoBox<TPixel> : Control where TPixel : unmanaged, IPixel<TPixel>
    {
        public VideoBox()
        {
            DefaultMediaOptions = OptionsUtil.CreateDefaultMediaOptions();
            DefaultResizeOptions = OptionsUtil.CreateDefaultResizeOption();
            DefaultResizeOptions.Mode = ResizeMode.Pad;
            ClientSize = new(64, 64);
            ContentAnchor = AnchorPosition.Centered;

            Played += OnPlayed;
            Paused += OnPaused;
            VideoFrameChanged += OnVideoFrameChanged;
        }

        public MediaOptions DefaultMediaOptions { get; }

        public ResizeOptions DefaultResizeOptions { get; }

        public MediaFilePlayer<TPixel>? MediaFilePlayer { get; private set; }

        public TimeSpan CurrentPosition => MediaFilePlayer?.CurrentPosition ?? TimeSpan.Zero;

        public TimeSpan TotalTime => MediaFilePlayer?.TotalTime ?? TimeSpan.Zero;

        public event EventHandler<VideoBox<TPixel>, EventArgs> Played;

        public event EventHandler<VideoBox<TPixel>, EventArgs> Paused;

        public event EventHandler<VideoBox<TPixel>, VideoFrameChangedEventArgs<TPixel>> VideoFrameChanged;

        protected virtual void OnPlayed(VideoBox<TPixel> sender, EventArgs e) { }

        protected virtual void OnPaused(VideoBox<TPixel> sender, EventArgs e) { }

        protected virtual void OnVideoFrameChanged(VideoBox<TPixel> sender, VideoFrameChangedEventArgs<TPixel> e)
        {
            RequestRendering();
        }

        protected override void OnAfterFrame(Control sender, EventArgs e)
        {
            base.OnAfterFrame(sender, e);

            MediaFilePlayer?.OnTickUpdate(MinecraftBlockScreen.Instance.SystemTick);
        }

        protected override BlockFrame Rendering()
        {
            VideoFrame<TPixel>? videoFrame = MediaFilePlayer?.CurrentVideoFrame;
            if (videoFrame is null)
                return base.Rendering();

            Texture<TPixel> texture = new(videoFrame.Image, DefaultResizeOptions);
            BlockFrame textureFrame = texture.CreateBlockFrame(ClientSize, GetScreenPlane().NormalFacing);
            if (IsRenderingTransparencyTexture)
                return textureFrame;

            BlockFrame baseFrame = base.Rendering();
            baseFrame.Overwrite(textureFrame, Point.Empty);
            return baseFrame;
        }

        private void MediaFilePlayer_Played(MediaFilePlayer<TPixel> sender, EventArgs e)
        {
            Played.Invoke(this, e);
        }

        private void MediaFilePlayer_Paused(MediaFilePlayer<TPixel> sender, EventArgs e)
        {
            Paused.Invoke(this, e);
        }

        private void MediaFilePlayer_VideoFrameChanged(MediaFilePlayer<TPixel> sender, VideoFrameChangedEventArgs<TPixel> e)
        {
            VideoFrameChanged.Invoke(this, e);
        }

        public bool TryReadMediaFile(string path)
        {
            if (!File.Exists(path))
                return false;

            try
            {
                MediaFilePlayer<TPixel> mediaFilePlayer = new(path, DefaultMediaOptions);
                if (MediaFilePlayer is not null)
                {
                    MediaFilePlayer.Played -= MediaFilePlayer_Played;
                    MediaFilePlayer.Paused -= MediaFilePlayer_Paused;
                    MediaFilePlayer.VideoFrameChanged -= MediaFilePlayer_VideoFrameChanged;
                    MediaFilePlayer.Dispose();
                }

                MediaFilePlayer = mediaFilePlayer;
                MediaFilePlayer.Played += MediaFilePlayer_Played;
                MediaFilePlayer.Paused += MediaFilePlayer_Paused;
                MediaFilePlayer.VideoFrameChanged += MediaFilePlayer_VideoFrameChanged;

                return true;
            }
            catch
            {
                return false;
            }
        }

        protected override void DisposeUnmanaged()
        {
            base.DisposeUnmanaged();

            MediaFilePlayer?.Dispose();
        }
    }
}
