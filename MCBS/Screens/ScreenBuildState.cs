﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCBS.Screens
{
    public enum ScreenBuildState
    {
        ReadStartPosition,

        ReadEndPosition,

        Timedout,

        Canceled,

        Completed
    }
}