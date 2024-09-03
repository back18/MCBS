using MCBS.Drawing;
using MCBS.Drawing.Extensions;
using QuanLib.Core;
using QuanLib.Core.Events;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.BlockForms
{
    public class SubtitleBox : Control
    {
        public SubtitleBox()
        {
            BorderWidth = 0;
            Skin.SetAllBackgroundColor(string.Empty);

            IsPlaying = false;
            IsLoopPlay = false;
            _UpdateInterval = 1;
            _PlayingSpeed = 1;
            _PlayingPosition = 0;
        }

        private HashBlockFrame? _textFrame;

        public bool IsPlaying { get; protected set; }

        public bool IsLoopPlay { get; set; }

        public int UpdateInterval
        {
            get => _UpdateInterval;
            set
            {
                ThrowHelper.ArgumentOutOfMin(1, value, nameof(value));
                _UpdateInterval = value;
            }
        }
        private int _UpdateInterval;

        public int PlayingSpeed
        {
            get => _PlayingSpeed;
            set
            {
                ThrowHelper.ArgumentOutOfMin(0, value, nameof(value));
                _PlayingSpeed = value;
            }
        }
        private int _PlayingSpeed;

        public int PlayingPosition
        {
            get => _PlayingPosition;
            set
            {
                if (_PlayingPosition != value)
                {
                    _PlayingPosition = value;
                    RequestRedraw();
                }
            }
        }
        private int _PlayingPosition;

        protected override void OnTextChanged(Control sender, ValueChangedEventArgs<string> e)
        {
            base.OnTextChanged(sender, e);

            _textFrame = null;
            Reset();
        }

        protected override void OnAfterFrame(Control sender, EventArgs e)
        {
            base.OnAfterFrame(sender, e);


            if (!IsPlaying || PlayingSpeed == 0)
                return;

            if (MinecraftBlockScreen.Instance.SystemTick % UpdateInterval != 0)
                return;

            Size textSize = SR.DefaultFont.GetTotalSize(Text);
            int position = PlayingPosition;

            if (IsLoopPlay)
            {
                int minPosition = -textSize.Width;
                if (position <= minPosition)
                    position = ClientSize.Width;
                else
                    position -= PlayingSpeed;
            }
            else
            {
                int minPosition = ClientSize.Width - textSize.Width;
                if (minPosition >= 0)
                    return;

                if (position <= minPosition)
                {
                    position = minPosition;
                    Pause();
                }
                else
                {
                    position -= PlayingSpeed;
                }
            }

            PlayingPosition = position;
        }

        protected override BlockFrame Drawing()
        {
            BlockFrame baseFrame = base.Drawing();
            if (string.IsNullOrEmpty(Text))
                return baseFrame;

            if (_textFrame is null)
            {
                _textFrame = new(SR.DefaultFont.GetTotalSize(Text));
                int x = 0;
                foreach (char c in Text)
                {
                    OverwriteContext overwriteContext = _textFrame.DrawBinary(SR.DefaultFont[c].GetBitArray(), GetForegroundColor().ToBlockId(), new(x, 0));
                    x = overwriteContext.BaseEndPosition.X + 1;
                }
            }

            baseFrame.Overwrite(_textFrame, new(PlayingPosition, 0));
            return baseFrame;
        }

        public void Play()
        {
            IsPlaying = true;
        }

        public void Pause()
        {
            IsPlaying = false;
        }

        public void Reset()
        {
            PlayingPosition = 0;
        }
    }
}
