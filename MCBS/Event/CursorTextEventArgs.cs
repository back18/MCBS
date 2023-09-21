﻿using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Event
{
    public class CursorTextEventArgs : CursorEventArgs
    {
        public CursorTextEventArgs(Point position, string text) : base(position)
        {
            Text = text;
        }

        public string Text { get; }
    }
}