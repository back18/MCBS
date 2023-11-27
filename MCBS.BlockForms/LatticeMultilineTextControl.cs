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
        static LatticeMultilineTextControl()
        {
            _idmap = new string[0x10000];
            for (int i = 0; i < _idmap.Length; i++)
                _idmap[i] = "lattice_block:lattice_block_" + i.ToString("x4");
        }

        protected LatticeMultilineTextControl()
        {
            BlockResolution = 4;
            FontPixelSize = 1;
            ScrollDelta = SR.DefaultFont.Height / BlockResolution * FontPixelSize;
        }

        private static readonly string[] _idmap;

        protected override string ToBlockId(int index)
        {
            return _idmap[index];
        }
    }
}
