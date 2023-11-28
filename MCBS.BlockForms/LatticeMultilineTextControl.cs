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
    public abstract class LatticeMultilineTextControl : AbstractMultilineTextControl
    {
        static LatticeMultilineTextControl()
        {
            _positive = new string[ushort.MaxValue];
            _negative = new string[ushort.MaxValue];
            for (int i = 0; i < ushort.MaxValue; i++)
            {
                ushort positive = (ushort)i;
                ushort negative = NumberUtil.ToUshort(NumberUtil.Negate(NumberUtil.ToBitArray((ushort)i)));
                _positive[i] = "lattice_block:lattice_block_" + positive.ToString("x4");
                _negative[i] = "lattice_block:lattice_block_" + negative.ToString("x4");
            }
        }

        protected LatticeMultilineTextControl()
        {
            IsNegativeMode = false;
            BlockResolution = 4;
            FontPixelSize = 1;
            ScrollDelta = SR.DefaultFont.Height / BlockResolution * FontPixelSize;
        }

        private static readonly string[] _positive;

        private static readonly string[] _negative;

        public bool IsNegativeMode { get; set; }

        protected override string ToBlockId(int index)
        {
            if (IsNegativeMode)
                return _negative[index];
            else
                return _positive[index];
        }
    }
}
