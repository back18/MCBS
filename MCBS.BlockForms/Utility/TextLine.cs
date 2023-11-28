using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.BlockForms.Utility
{
    public readonly struct TextLine(string text, int paragraphNumber, int lineNumber, int textIndex)
    {
        public readonly string Text = text;

        public readonly int ParagraphNumber = paragraphNumber;

        public readonly int LineNumber = lineNumber;

        public readonly int TextIndex = textIndex;
    }
}
