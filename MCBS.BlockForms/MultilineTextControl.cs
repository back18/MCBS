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
    public abstract class MultilineTextControl : AbstractMultilineTextControl
    {
        protected override BlockFrame Rendering()
        {
            BlockFrame baseFrame = base.Rendering();
            if (Lines.Count == 0)
                return baseFrame;

            Point start = OffsetPosition;
            Point end = new(OffsetPosition.X + ClientSize.Width - 1, OffsetPosition.Y + ClientSize.Height - 1);
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
