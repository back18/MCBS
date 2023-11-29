using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.BlockForms.Utility
{
    public class TextParagraph
    {
        public TextParagraph()
        {
            Text = string.Empty;
            Lines = new(new string[] { string.Empty });
            ParagraphNumber = 0;
            TextIndex = 0;
            LineBreakType = LineBreak.None;
        }

        public TextParagraph(string text, IList<string> lines, int paragraphNumber, int textIndex, LineBreak lineBreakType)
        {
            ArgumentNullException.ThrowIfNull(text, nameof(text));
            ArgumentNullException.ThrowIfNull(lines, nameof(lines));

            Text = text;
            Lines = new(lines);
            ParagraphNumber = paragraphNumber;
            TextIndex = textIndex;
            LineBreakType = lineBreakType;
        }

        public string Text { get; }

        public ReadOnlyCollection<string> Lines { get; }

        public int ParagraphNumber { get; }

        public int TextIndex { get; }

        public LineBreak LineBreakType { get; }

        public string LineBreakString
        {
            get
            {
                return LineBreakType switch
                {
                    LineBreak.None => string.Empty,
                    LineBreak.CR => "\r",
                    LineBreak.LF => "\n",
                    LineBreak.CRLF => "\r\n",
                    _ => throw new InvalidOperationException(),
                };
            }
        }

        public int LineBreakLength
        {
            get
            {
                return LineBreakType switch
                {
                    LineBreak.None => 0,
                    LineBreak.CR => 1,
                    LineBreak.LF => 1,
                    LineBreak.CRLF => 2,
                    _ => throw new InvalidOperationException(),
                };
            }
        }
    }
}
