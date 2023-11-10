using MCBS.Events;
using MCBS.Rendering;
using QuanLib.BDF;
using QuanLib.Core.Events;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.BlockForms
{
    public abstract class MultilineTextControl : ScrollablePanel
    {
        protected MultilineTextControl()
        {
            Lines = new(Array.Empty<string>());
            _WordWrap = true;
            _AutoScroll = false;
        }

        public ReadOnlyCollection<string> Lines { get; private set; }

        public bool WordWrap
        {
            get => _WordWrap;
            set
            {
                if (_WordWrap != value)
                {
                    _WordWrap = value;
                    UpdatePageSize();
                }
            }
        }
        private bool _WordWrap;

        public bool AutoScroll
        {
            get => _AutoScroll;
            set
            {
                if (_AutoScroll != value)
                {
                    _AutoScroll = value;
                    UpdatePageSize();
                }
            }
        }
        private bool _AutoScroll;

        protected override BlockFrame Rendering()
        {
            BlockFrame baseFrame = base.Rendering();
            if (Lines.Count == 0)
                return baseFrame;

            Point start = OffsetPosition;
            Point end = new(OffsetPosition.X + ClientSize.Width, OffsetPosition.Y + ClientSize.Height);
            Point position = new(0, start.Y / SR.DefaultFont.Height * SR.DefaultFont.Height);
            for (int i = start.Y / SR.DefaultFont.Height; i < Lines.Count; i++)
            {
                if (position.Y > end.Y)
                    break;

                foreach (char c in Lines[i])
                {
                    FontData fontData = SR.DefaultFont[c];
                    if (position.X > end.X)
                        break;
                    if (position.X + fontData.Width < start.X)
                    {
                        position.X += fontData.Width;
                        continue;
                    }

                    baseFrame.DrawBinary(fontData.GetBinary(), GetForegroundColor().ToBlockId(), position);
                    position.X += fontData.Width;
                }

                position.X = 0;
                position.Y += SR.DefaultFont.Height;
            }

            return baseFrame;
        }

        protected override void OnResize(Control sender, SizeChangedEventArgs e)
        {
            base.OnResize(sender, e);

            if (AutoSize || WordWrap)
                UpdatePageSize();
        }

        protected override void OnTextChanged(Control sender, TextChangedEventArgs e)
        {
            base.OnTextChanged(sender, e);

            UpdatePageSize();

            if (AutoScroll)
            {
                int y = PageSize.Height - ClientSize.Height;
                if (y > 0)
                    OffsetPosition = new(OffsetPosition.X, y);
            }
        }

        public override void AutoSetSize()
        {
            UpdatePageSize();
        }

        protected virtual void UpdatePageSize()
        {
            string[] lines = Text.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            if (AutoSize || !WordWrap)
            {
                int maxWidth = 0;
                foreach (string line in lines)
                {
                    int width = SR.DefaultFont.GetTotalSize(line).Width;
                    if (width > maxWidth)
                        maxWidth = width;
                }

                Lines = new(lines);
                PageSize = new(maxWidth, Lines.Count * SR.DefaultFont.Height);

                if (AutoSize)
                    ClientSize = PageSize;
            }
            else
            {
                if (ClientSize.Width < SR.DefaultFont.FullWidth)
                {
                    Lines = new(Array.Empty<string>());
                    PageSize = ClientSize;
                }

                List<string> words = new();
                foreach (string line in lines)
                {
                    int start = 0;
                    int width = 0;
                    for (int i = 0; i < line.Length; i++)
                    {
                        var data = SR.DefaultFont[line[i]];
                        width += data.Width;
                        if (width > ClientSize.Width)
                        {
                            words.Add(line[start..i]);
                            start = i;
                            width = data.Width;
                        }
                    }
                    words.Add(line[start..line.Length]);
                }

                Lines = new(words);
                PageSize = new(ClientSize.Width, Lines.Count * SR.DefaultFont.Height);
            }
        }
    }
}
