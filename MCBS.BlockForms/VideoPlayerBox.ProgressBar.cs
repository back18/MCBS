using MCBS.Events;
using QuanLib.Core.Events;
using QuanLib.Minecraft.Blocks;
using QuanLib.TickLoop.VideoPlayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.BlockForms
{
    public partial class VideoPlayerBox<TPixel>
    {
        public class VideoProgressBar : HorizontalProgressBar
        {
            public VideoProgressBar(VideoPlayerBox<TPixel> owner)
            {
                ArgumentNullException.ThrowIfNull(owner, nameof(owner));

                _owner = owner;

                Time_Label = new();

                Skin.SetAllForegroundColor(BlockManager.Concrete.Pink);

                _owner.VideoBox.VideoFrameChanged += VideoPlayer_VideoFrameChanged;
            }

            protected readonly VideoPlayerBox<TPixel> _owner;

            private readonly Label Time_Label;

            private void VideoPlayer_VideoFrameChanged(VideoBox<TPixel> sender, ValueChangedEventArgs<VideoFrame<TPixel>?> e)
            {
                RequestRedraw();
                Progress = _owner.VideoBox.CurrentPosition / _owner.VideoBox.TotalTime;
            }

            protected override void OnCursorMove(Control sender, CursorEventArgs e)
            {
                base.OnCursorMove(sender, e);

                if (_owner.ChildControls.Contains(Time_Label))
                {
                    Time_Label.Skin.SetAllBackgroundColor(BlockManager.Concrete.White);
                    Time_Label.Text = FormatTime(GetProgressBarPosition(e.Position.X));
                    Time_Label.AutoSetSize();
                    int x = e.Position.X - Time_Label.Width / 2;
                    Time_Label.ClientLocation = new(x, TopLocation - Time_Label.Height - 2);
                }
            }

            protected override void OnCursorEnter(Control sender, CursorEventArgs e)
            {
                base.OnCursorEnter(sender, e);

                _owner.ChildControls.TryAdd(Time_Label);
            }

            protected override void OnCursorLeave(Control sender, CursorEventArgs e)
            {
                base.OnCursorLeave(sender, e);

                _owner.ChildControls.Remove(Time_Label);
            }

            protected override void OnRightClick(Control sender, CursorEventArgs e)
            {
                base.OnRightClick(sender, e);

                if (_owner.VideoBox.MediaFilePlayer is null)
                    return;

                _owner.VideoBox.MediaFilePlayer.CurrentPosition = GetProgressBarPosition(e.Position.X);
            }

            private TimeSpan GetProgressBarPosition(int x)
            {
                return _owner.VideoBox.TotalTime * ((double)x / ClientSize.Width);
            }

            private static string FormatTime(TimeSpan time)
            {
                string result = string.Empty;
                if (time.Hours > 0)
                    result += time.Hours.ToString().PadLeft(2, '0') + ':';
                result += $"{time.Minutes.ToString().PadLeft(2, '0')}:{time.Seconds.ToString().PadLeft(2, '0')}";
                return result;
            }
        }
    }
}
