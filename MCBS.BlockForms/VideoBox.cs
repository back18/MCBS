using FFMediaToolkit.Decoding;
using MCBS.BlockForms.Utility;
using MCBS.Drawing;
using MCBS.Drawing.Extensions;
using MCBS.UI.Extensions;
using QuanLib.Core;
using QuanLib.Core.Events;
using QuanLib.TickLoop.VideoPlayer;
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

        private BlockFrame? _preDrawFrame;

        public MediaOptions DefaultMediaOptions { get; }

        public ResizeOptions DefaultResizeOptions { get; }

        public MediaFilePlayer<TPixel>? MediaFilePlayer { get; private set; }

        public TimeSpan CurrentPosition => MediaFilePlayer?.CurrentPosition ?? TimeSpan.Zero;

        public TimeSpan TotalTime => MediaFilePlayer?.TotalTime ?? TimeSpan.Zero;

        public event EventHandler<VideoBox<TPixel>, EventArgs> Played;

        public event EventHandler<VideoBox<TPixel>, EventArgs> Paused;

        public event EventHandler<VideoBox<TPixel>, ValueChangedEventArgs<VideoFrame<TPixel>?>> VideoFrameChanged;

        protected virtual void OnPlayed(VideoBox<TPixel> sender, EventArgs e) { }

        protected virtual void OnPaused(VideoBox<TPixel> sender, EventArgs e) { }

        protected virtual void OnVideoFrameChanged(VideoBox<TPixel> sender, ValueChangedEventArgs<VideoFrame<TPixel>?> e)
        {
            _preDrawFrame = PreDrawing();
            RequestRedraw();
        }

        protected override void OnAfterFrame(Control sender, EventArgs e)
        {
            base.OnAfterFrame(sender, e);

            MediaFilePlayer?.OnTickUpdate(MinecraftBlockScreen.Instance.SystemTick);
        }

        protected override BlockFrame Drawing()
        {
            if (_preDrawFrame is null)
                return base.Drawing();

            return _preDrawFrame;
        }

        private BlockFrame? PreDrawing()
        {
            VideoFrame<TPixel>? videoFrame = MediaFilePlayer?.CurrentVideoFrame;
            if (videoFrame is null)
                return null;

            Texture<TPixel> texture = new(videoFrame.Image, DefaultResizeOptions);
            BlockFrame textureFrame = texture.CreateBlockFrame(ClientSize, this.GetNormalFacing());
            if (RequestDrawTransparencyTexture)
                return textureFrame;

            BlockFrame baseFrame = base.Drawing();
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

        private void MediaFilePlayer_VideoFrameChanged(MediaFilePlayer<TPixel> sender, ValueChangedEventArgs<VideoFrame<TPixel>?> e)
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
