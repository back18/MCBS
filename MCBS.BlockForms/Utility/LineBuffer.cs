using QuanLib.BDF;
using QuanLib.Core;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.BlockForms.Utility
{
    public class LineBuffer
    {
        public LineBuffer()
        {
            Text = string.Empty;
            BufferSize = Size.Empty;

            _paragraphs = [];
            _lines = [];
        }

        private readonly List<TextParagraph> _paragraphs;

        private readonly List<TextLine> _lines;

        public string Text { get; private set; }

        public Size BufferSize { get; private set; }

        public IReadOnlyList<TextParagraph> Paragraphs => _paragraphs;

        public IReadOnlyList<TextLine> Lines => _lines;

        public void UpdateText(string text, BdfFont bdfFont, int pixelSize)
        {
            ArgumentNullException.ThrowIfNull(text, nameof(text));
            ArgumentNullException.ThrowIfNull(bdfFont, nameof(bdfFont));
            ThrowHelper.ArgumentOutOfMin(1, pixelSize, nameof(pixelSize));

            _paragraphs.Clear();
            _lines.Clear();
            Text = text;

            List<(string text, int index, LineBreak lineBreak)> lines = SplitLines(Text);
            for (int i = 0; i < lines.Count; i++)
            {
                var line = lines[i];
                _paragraphs.Add(new(line.text, new string[] { line.text }, i, line.index, line.lineBreak));
                _lines.Add(new(line.text, i, i, line.index));
            }

            int maxWidth = 0;
            foreach (var line in _lines)
            {
                int width = bdfFont.GetTotalSize(line.Text).Width;
                if (width > maxWidth)
                    maxWidth = width;
            }

            BufferSize = new Size(maxWidth, bdfFont.Height * _lines.Count) * pixelSize;
        }

        public void UpdateText(string text, BdfFont bdfFont, int pixelSize, int maxWidth)
        {
            ArgumentNullException.ThrowIfNull(text, nameof(text));
            ArgumentNullException.ThrowIfNull(bdfFont, nameof(bdfFont));
            ThrowHelper.ArgumentOutOfMin(1, pixelSize, nameof(pixelSize));

            _paragraphs.Clear();
            _lines.Clear();
            Text = text;

            List<(string text, int index, LineBreak lineBreak)> lines = SplitLines(Text);

            if (maxWidth < bdfFont.FullWidth)
            {
                BufferSize = Size.Empty;
                return;
            }

            for (int i = 0; i < lines.Count; i++)
            {
                var line = lines[i];
                int start = 0;
                int width = 0;

                List<string> texts = [];
                for (int j = 0; j < line.text.Length; j++)
                {
                    FontData fontData = bdfFont[line.text[j]];
                    int fontWidth = fontData.Width * pixelSize;
                    width += fontWidth;
                    if (width > maxWidth)
                    {
                        texts.Add(line.text[start..j]);
                        start = j;
                        width = fontWidth;
                    }
                }
                texts.Add(line.text[start..line.text.Length]);

                _paragraphs.Add(new(line.text, texts, i, line.index, line.lineBreak));

                int index = line.index;
                for (int j = 0; j < texts.Count; j++)
                {
                    _lines.Add(new(texts[j], i, _lines.Count, index));
                    index += texts[j].Length;
                }
            }

            BufferSize = new(maxWidth, bdfFont.Height * _lines.Count * pixelSize);
        }

        private static List<(string text, int index, LineBreak lineBreak)> SplitLines(string text)
        {
            ArgumentNullException.ThrowIfNull(text, nameof(text));

            StringBuilder buffer = new();
            int index = 0;
            List<(string text, int index, LineBreak lineBreak)> result = [];
            for (int i = 0; i < text.Length; i++)
            {
                switch (text[i])
                {
                    case '\r':
                        result.Add((buffer.ToString(), index, LineBreak.CR));
                        buffer.Clear();
                        index = i + 1;
                        break;
                    case '\n':
                        if (buffer.Length == 0 && result.Count > 0 && result[^1].lineBreak == LineBreak.CR)
                        {
                            result[^1] = (result[^1].text, result[^1].index, LineBreak.CRLF);
                            index = i + 1;
                        }
                        else
                        {
                            result.Add((buffer.ToString(), index, LineBreak.LF));
                            buffer.Clear();
                            index = i + 1;
                        }
                        break;
                    default:
                        buffer.Append(text[i]);
                        break;
                }
            }

            if (buffer.Length > 0)
                result.Add((buffer.ToString(), index, LineBreak.None));

            return result;
        }
    }
}
