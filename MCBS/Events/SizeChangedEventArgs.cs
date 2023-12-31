﻿using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Events
{
    public class SizeChangedEventArgs : EventArgs
    {
        public SizeChangedEventArgs(Size oldSize, Size newSize)
        {
            OldSize = oldSize;
            NewSize = newSize;
        }

        public Size OldSize { get; }

        public Size NewSize { get; }
    }
}
