using iNKORE.UI.WPF.Modern.Controls;
using QuanLib.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace MCBS.WpfApp.Converters
{
    public class NumberFormatter : INumberBoxNumberFormatter
    {
        public NumberFormatter(int digits, MidpointRounding mode = MidpointRounding.ToEven)
        {
            ThrowHelper.ArgumentOutOfRange(0, 15, digits, nameof(digits));

            Digits = digits;
            Mode = mode;
        }

        public int Digits { get; }

        public MidpointRounding Mode { get; }

        public string FormatDouble(double value)
        {
            value = Math.Round(value, Digits, Mode);
            return value.ToString();
        }

        public double? ParseDouble(string text)
        {
            if (double.TryParse(text, out double result))
            {
                result = Math.Round(result, Digits, Mode);
                return result;
            }

            return null;
        }
    }
}
