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
    public abstract class LatticeMultilineTextControl : AbstractMultilineTextControl
    {
        protected override BlockFrame Rendering()
        {
            Size renderingSize = GetRenderingSize();
            HashBlockFrame baseFrame = new HashBlockFrame(renderingSize, ToLatticeBlockId(0));
            if (Lines.Count == 0)
                return baseFrame;

            Size bufferSize = ScreenSize2BufferSize(renderingSize);
            bool[,] buffer = new bool[bufferSize.Width, bufferSize.Height];
            Point start = ScreenPos2BufferPos(OffsetPosition);
            Point end = ScreenPos2BufferPos(new(OffsetPosition.X + ClientSize.Width - 1, OffsetPosition.Y + ClientSize.Height - 1));
            end.Offset(3, 3);
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

                    var binary = fontData.GetBinary();
                    Size fontSize = new(fontData.Width, fontData.Height);
                    OverwriteContext overwriteContext = new(new(bufferSize.Width, bufferSize.Height), fontSize, fontSize, position, Point.Empty);
                    foreach (var mapping in overwriteContext)
                    {
                        if (binary[mapping.OverwritePosition.X, mapping.OverwritePosition.Y])
                            buffer[mapping.BasePosition.X, mapping.BasePosition.Y] = true;
                    }

                    position.X += fontSize.Width;
                }

                position.X = 0;
                position.Y += SR.DefaultFont.Height;
            }

            for (int y1 = start.Y; y1 <= end.Y; y1 += 4)
                for (int x1 = start.X; x1 <= end.X; x1 += 4)
                {
                    int total = 0;
                    int value = 0x8000;
                    int yed = y1 + 4;
                    int xed = x1 + 4;
                    for (int y2 = y1; y2 < yed; y2++)
                        for (int x2 = x1; x2 < xed; x2++)
                        {
                            if (buffer[x2, y2])
                                total += value;
                            value /= 2;
                        }

                    baseFrame[x1 / 4, y1 / 4] = ToLatticeBlockId(total);
                }

            return baseFrame;
        }

        protected override void UpdatePageSize()
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
                PageSize = BufferSize2ScreenSize(new(maxWidth, Lines.Count * SR.DefaultFont.Height));

                if (AutoSize)
                    ClientSize = PageSize;
            }
            else
            {
                int totalWidth = ScreenSize2BufferSize(ClientSize).Width;
                if (totalWidth < SR.DefaultFont.FullWidth)
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
                        if (width > totalWidth)
                        {
                            words.Add(line[start..i]);
                            start = i;
                            width = data.Width;
                        }
                    }
                    words.Add(line[start..line.Length]);
                }

                Lines = new(words);
                PageSize = BufferSize2ScreenSize(new(ClientSize.Width, Lines.Count * SR.DefaultFont.Height));
            }
        }

        private static Size BufferSize2ScreenSize(Size size)
        {
            return new((int)Math.Ceiling(size.Width / 4.0), (int)Math.Ceiling(size.Height / 4.0));
        }

        private static Size ScreenSize2BufferSize(Size size)
        {
            return new(size.Width * 4, size.Height * 4);
        }

        private static Point BufferPos2ScreenPos(Point position)
        {
            return new(position.X / 4, position.Y / 4);
        }

        private static Point ScreenPos2BufferPos(Point position)
        {
            return new(position.X * 4, position.Y * 4);
        }

        private static string ToLatticeBlockId(int index)
        {
            return "lattice_block:lattice_block_" + index.ToString("x4");
        }
    }
}
