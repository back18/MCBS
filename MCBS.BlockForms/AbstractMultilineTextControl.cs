using MCBS.Events;
using MCBS.Rendering;
using QuanLib.BDF;
using QuanLib.Core;
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
    public abstract class AbstractMultilineTextControl : ScrollablePanel
    {
        protected AbstractMultilineTextControl()
        {
            TextBufffer = new();
            Lines = new(Array.Empty<string>());
            BlockResolution = 1;
            FontPixelSize = 1;
            ScrollDelta = SR.DefaultFont.Height / BlockResolution * FontPixelSize;
            _WordWrap = true;
            _AutoScroll = false;
        }

        public override string Text
        {
            get => base.Text;
            set
            {
                base.Text = value;
                string temp = TextBufffer.ToString();
                if (temp != value)
                {
                    TextBufffer.Clear();
                    TextBufffer.Append(value);
                }
            }
        }

        public StringBuilder TextBufffer { get; }

        public virtual ReadOnlyCollection<string> Lines { get; protected set; }

        public int BlockResolution { get; protected set; }

        public int FontPixelSize { get; protected set; }

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
            Size renderingSize = GetRenderingSize();
            HashBlockFrame baseFrame = new(renderingSize, ToBlockId(0));
            if (Lines.Count == 0)
                return baseFrame;

            (bool[,] buffer, Rectangle rectangle) build = BuildBuffer();
            int pow = (int)Math.Pow(2, BlockResolution * BlockResolution - 1);

            for (int y1 = build.rectangle.Y / BlockResolution, y2 = 0; y2 < build.rectangle.Height; y1++, y2 += BlockResolution)
                for (int x1 = build.rectangle.X / BlockResolution, x2 = 0; x2 < build.rectangle.Width; x1++, x2 += BlockResolution)
                {
                    int total = 0;
                    int value = pow;
                    int yend = y2 + BlockResolution;
                    int xend = x2 + BlockResolution;
                    for (int y3 = y2; y3 < yend; y3++)
                        for (int x3 = x2; x3 < xend; x3++)
                        {
                            if (build.buffer[x3, y3])
                                total += value;
                            value /= 2;
                        }

                    baseFrame[x1, y1] = ToBlockId(total);
                }

            return baseFrame;
        }

        protected virtual (bool[,] buffer, Rectangle rectangle) BuildBuffer()
        {
            int fontHeight = SR.DefaultFont.Height * FontPixelSize;
            Point start = PagePosBufferPos(OffsetPosition);
            Size size = PageSize2BufferSize(ClientSize);
            Point end = new(start.X + size.Width - 1, start.Y + size.Height - 1);
            Rectangle rectangle = new(start.X, start.Y, size.Width, size.Height);
            Point position = new(0, start.Y / fontHeight * fontHeight);
            bool[,] buffer = new bool[rectangle.Width, rectangle.Height];

            for (int i = start.Y / fontHeight; i < Lines.Count; i++)
            {
                if (position.Y > end.Y)
                    break;

                foreach (char c in Lines[i])
                {
                    FontData fontData = SR.DefaultFont[c];
                    Size fontSize = new Size(fontData.Width, fontData.Height) * FontPixelSize;

                    if (position.X > end.X)
                        break;
                    if (position.X + fontSize.Width < start.X)
                    {
                        position.X += fontSize.Width;
                        continue;
                    }

                    var bitmap = fontData.GetBitArray(FontPixelSize);
                    OverwriteContext overwriteContext = new(new(rectangle.Width, rectangle.Height), fontSize, fontSize, new(position.X - start.X, position.Y - start.Y), Point.Empty);
                    foreach (var mapping in overwriteContext)
                    {
                        if (bitmap[mapping.OverwritePosition.X, mapping.OverwritePosition.Y])
                            buffer[mapping.BasePosition.X, mapping.BasePosition.Y] = true;
                    }

                    position.X += fontSize.Width;
                }

                position.X = 0;
                position.Y += fontHeight;
            }

            return (buffer, rectangle);
        }
        
        protected virtual void UpdatePageSize()
        {
            string[] lines = Text.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

            if (AutoSize || !WordWrap)
            {
                int maxWidth = 0;
                foreach (string line in lines)
                {
                    int width = SR.DefaultFont.GetTotalSize(line).Width * FontPixelSize;
                    if (width > maxWidth)
                        maxWidth = width;
                }

                Lines = new(lines);
                PageSize = BufferSize2PageSize(new(Math.Max(ClientSize.Width * BlockResolution, maxWidth), Lines.Count * SR.DefaultFont.Height * FontPixelSize));

                if (AutoSize)
                    ClientSize = PageSize;
            }
            else
            {
                int totalWidth = PageSize2BufferSize(ClientSize).Width;
                if (totalWidth < SR.DefaultFont.FullWidth)
                {
                    Lines = new(Array.Empty<string>());
                    PageSize = ClientSize;
                }

                List<string> words = [];
                foreach (string line in lines)
                {
                    int start = 0;
                    int width = 0;
                    for (int i = 0; i < line.Length; i++)
                    {
                        var data = SR.DefaultFont[line[i]];
                        width += data.Width * FontPixelSize;
                        if (width > totalWidth)
                        {
                            words.Add(line[start..i]);
                            start = i;
                            width = data.Width * FontPixelSize;
                        }
                    }
                    words.Add(line[start..line.Length]);
                }

                Lines = new(words);
                PageSize = BufferSize2PageSize(new(ClientSize.Width, Lines.Count * SR.DefaultFont.Height * FontPixelSize));
            }
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

        public void TextBuffferUpdated()
        {
            base.Text = TextBufffer.ToString();
        }

        protected abstract string ToBlockId(int index);

        protected Point BufferPos2PagePos(Point position)
        {
            return new(
                NumberUtil.DivisionFloor(position.X , BlockResolution),
                NumberUtil.DivisionFloor(position.Y , BlockResolution));
        }

        protected Point PagePosBufferPos(Point position)
        {
            return new Point(position.X, position.Y) * BlockResolution;
        }

        protected Size BufferSize2PageSize(Size size)
        {
            return new(
                NumberUtil.DivisionCeiling(size.Width, BlockResolution),
                NumberUtil.DivisionCeiling(size.Height, BlockResolution));
        }

        protected Size PageSize2BufferSize(Size size)
        {
            return new Size(size.Width, size.Height) * BlockResolution;
        }
    }
}
