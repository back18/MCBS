using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MCBS.BlockForms.DialogBox;
using MCBS.BlockForms.Utility;
using MCBS.Events;
using MCBS.UI.Extensions;
using MCBS.UI;
using QuanLib.Core.Events;
using QuanLib.Minecraft.Blocks;
using QuanLib.TickLoop.VideoPlayer;

namespace MCBS.BlockForms
{
    public partial class VideoPlayerBox<TPixel>
    {
        public class TimeTextBox : ContainerControl<Control>
        {
            public TimeTextBox(VideoPlayerBox<TPixel> owner)
            {
                ArgumentNullException.ThrowIfNull(owner, nameof(owner));

                _owner = owner;

                CurrentTime_TextBox = new();
                TotalTime_Label = new();

                BorderWidth = 0;
                Skin.SetAllBackgroundColor(string.Empty);

                _owner.VideoBox.VideoFrameChanged += VideoPlayer_VideoFrameChanged;
            }

            protected readonly VideoPlayerBox<TPixel> _owner;

            private readonly TextBox CurrentTime_TextBox;

            private readonly Label TotalTime_Label;

            public override void Initialize()
            {
                base.Initialize();

                if (_owner != ParentContainer)
                    throw new InvalidOperationException();

                ChildControls.Add(CurrentTime_TextBox);
                CurrentTime_TextBox.BorderWidth = 0;
                CurrentTime_TextBox.AutoSize = true;
                CurrentTime_TextBox.Text = FormatTime(_owner.VideoBox.CurrentPosition);
                CurrentTime_TextBox.Skin.SetAllForegroundColor(BlockManager.Concrete.Pink);
                CurrentTime_TextBox.Skin.SetBackgroundColor(string.Empty, ControlState.None, ControlState.Selected);
                CurrentTime_TextBox.TextEditorUpdate += CurrentTime_TextBox_TextEditorUpdate;

                ChildControls.Add(TotalTime_Label);
                TotalTime_Label.BorderWidth = 0;
                TotalTime_Label.AutoSize = true;
                TotalTime_Label.Text = '/' + FormatTime(_owner.VideoBox.TotalTime);
                TotalTime_Label.Skin.SetAllForegroundColor(BlockManager.Concrete.Pink);
            }

            private void CurrentTime_TextBox_TextEditorUpdate(Control sender, CursorEventArgs e)
            {
                if (e.NewData.TextEditor == FormatTime(_owner.VideoBox.CurrentPosition))
                    return;

                string timeText = e.NewData.TextEditor;
                string[] times = timeText.Split(':');
                if (times.Length == 1)
                {
                    timeText = "00:00:" + timeText;
                }
                else if (times.Length == 2)
                {
                    timeText = "00:" + timeText;
                }

                MediaFilePlayer<TPixel>? mediaFilePlayer = _owner.VideoBox.MediaFilePlayer;
                if (mediaFilePlayer is null ||
                    !TimeSpan.TryParse(timeText, out var time))
                {
                    IForm? form = this.GetForm();
                    if (form is not null)
                        _ = DialogBoxHelper.OpenMessageBoxAsync(form, "警告", $"无法跳转到：“{timeText}”", MessageBoxButtons.OK);
                    return;
                }

                mediaFilePlayer.JumpToFrame(time);
            }

            public override void AfterInitialize()
            {
                base.AfterInitialize();

                ActiveLayoutAll();
            }

            private void VideoPlayer_VideoFrameChanged(VideoBox<TPixel> sender, ValueChangedEventArgs<VideoFrame<TPixel>?> e)
            {
                RequestRedraw();
                ActiveLayoutAll();
            }

            public override void ActiveLayoutAll()
            {
                CurrentTime_TextBox.Text = FormatTime(_owner.VideoBox.CurrentPosition);
                TotalTime_Label.Text = '/' + FormatTime(_owner.VideoBox.TotalTime);
                TotalTime_Label.LayoutRight(this, CurrentTime_TextBox, 0);
                Width = CurrentTime_TextBox.Width + TotalTime_Label.Width;
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
