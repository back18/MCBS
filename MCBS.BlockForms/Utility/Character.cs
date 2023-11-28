using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.BlockForms.Utility
{
    public readonly struct Character(char @char, int lineNumber, int columnNumber, Rectangle rectangle)
    {
        public readonly char Char = @char;

        public readonly int LineNumber = lineNumber;

        public readonly int ColumnNumber = columnNumber;

        public readonly Rectangle Rectangle = rectangle;
    }
}
