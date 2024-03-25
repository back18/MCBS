﻿using MCBS.Cursor;
using QuanLib.Game;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Forms
{
    public class StretchingContext
    {
        public StretchingContext(CursorContext cursorContext, Direction borders)
        {
            ArgumentNullException.ThrowIfNull(cursorContext, nameof(cursorContext));

            CursorContext = cursorContext;
            Borders = borders;
        }

        public CursorContext CursorContext { get; }

        public Direction Borders { get; }
    }
}
