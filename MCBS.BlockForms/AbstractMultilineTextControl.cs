using MCBS.BlockForms.Utility;
using MCBS.Rendering;
using QuanLib.BDF;
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
    public abstract class AbstractMultilineTextControl : ScrollablePanel
    {
        protected AbstractMultilineTextControl()
        {
            TextBuffer = new();
            LineBuffer = new();
            HighlightedCharacters = [];
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
                string temp = TextBuffer.ToString();
                if (temp != value)
                {
                    TextBuffer.Clear();
                    TextBuffer.Append(value);
                }
            }
        }

        public StringBuilder TextBuffer { get; }

        public LineBuffer LineBuffer { get; }

        public HashSet<int> HighlightedCharacters { get; }

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
            if (LineBuffer.Lines.Count == 0)
                return baseFrame;

            (bool[,] buffer, Rectangle rectangle) build = BuildBuffer();
            int pow = (int)Math.Pow(2, BlockResolution * BlockResolution - 1);

            for (int y1 = build.rectangle.Y / BlockResolution, y2 = 0; y1 < baseFrame.Height && y2 < build.rectangle.Height; y1++, y2 += BlockResolution)
                for (int x1 = build.rectangle.X / BlockResolution, x2 = 0; x1 < baseFrame.Width && x2 < build.rectangle.Width; x1++, x2 += BlockResolution)
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
            int lineNumber = start.Y / fontHeight;
            Point position = new(0, lineNumber * fontHeight);
            bool[,] buffer = new bool[rectangle.Width, rectangle.Height];

            for (int i = lineNumber; i < LineBuffer.Lines.Count; i++)
            {
                if (position.Y > end.Y)
                    break;

                TextLine line = LineBuffer.Lines[i];
                for (int j = 0; j < line.Text.Length; j++)
                {
                    FontData fontData = SR.DefaultFont[line.Text[j]];
                    Size fontSize = new Size(fontData.Width, fontData.Height) * FontPixelSize;

                    if (position.X > end.X)
                        break;
                    if (position.X + fontSize.Width < start.X)
                    {
                        position.X += fontSize.Width;
                        continue;
                    }

                    bool isNegative = HighlightedCharacters.Contains(line.TextIndex + j);
                    var bitmap = fontData.GetBitArray(FontPixelSize, isNegative);
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
            if (AutoSize || !WordWrap)
            {
                LineBuffer.UpdateText(Text, SR.DefaultFont, FontPixelSize);
                PageSize = BufferSize2PageSize(new(Math.Max(ClientSize.Width * BlockResolution, LineBuffer.BufferSize.Width), LineBuffer.BufferSize.Height));

                if (AutoSize)
                    ClientSize = PageSize;
            }
            else
            {
                LineBuffer.UpdateText(Text, SR.DefaultFont, FontPixelSize, ClientSize.Width * BlockResolution);
                PageSize = BufferSize2PageSize(LineBuffer.BufferSize);
            }
        }

        protected override void OnResize(Control sender, ValueChangedEventArgs<Size> e)
        {
            base.OnResize(sender, e);

            if (AutoSize || WordWrap)
                UpdatePageSize();
        }

        protected override void OnTextChanged(Control sender, ValueChangedEventArgs<string> e)
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

        public void TextBufferUpdated()
        {
            base.Text = TextBuffer.ToString();
        }

        protected abstract string ToBlockId(int index);

        protected Point BufferPos2PagePos(Point position)
        {
            return new(
                NumberUtil.DivisionFloor(position.X, BlockResolution),
                NumberUtil.DivisionFloor(position.Y, BlockResolution));
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

        public Character GetCharacter(Point bufferPosition)
        {
            int fontHeight = SR.DefaultFont.Height * FontPixelSize;
            int lineNumber = Math.Clamp(bufferPosition.Y / fontHeight, 0, Math.Max(0, LineBuffer.Lines.Count - 1));
            bufferPosition.Y = lineNumber * fontHeight;

            if (lineNumber < LineBuffer.Lines.Count)
            {
                string line = LineBuffer.Lines[lineNumber].Text;
                int width = 0;
                for (int i = 0; i < line.Length; i++)
                {
                    FontData fontData = SR.DefaultFont[line[i]];
                    int fontWudth = fontData.Width * FontPixelSize;
                    if (width + fontWudth > bufferPosition.X || i + 1 == line.Length)
                    {
                        char c = line[i];
                        bufferPosition.X = width;
                        int columnNumber = i;
                        Rectangle rectangle = new(bufferPosition.X, bufferPosition.Y, fontData.Width * FontPixelSize, fontData.Height * FontPixelSize);
                        return new(c, lineNumber, columnNumber, rectangle);
                    }

                    width += fontData.Width * FontPixelSize;
                }
            }

            bufferPosition.X = 0;
            return new('\0', lineNumber, 0, new(bufferPosition.X, bufferPosition.Y, 0, 0));
        }
    }
}
